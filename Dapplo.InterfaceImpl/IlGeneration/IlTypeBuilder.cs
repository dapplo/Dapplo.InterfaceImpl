//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.InterfaceImpl
// 
//  Dapplo.InterfaceImpl is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.InterfaceImpl is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.InterfaceImpl. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dapplo.Log;
using System.Collections.Generic;

#endregion

namespace Dapplo.InterfaceImpl.IlGeneration
{
	/// <summary>
	/// Use Il to build a type for a specified interface
	/// </summary>
	public class IlTypeBuilder
	{
		private static readonly LogSource Log = new LogSource();
		private static readonly IDictionary<Tuple<bool, string>, IlTypeBuilder> IlTypeBuilderCache = new Dictionary<Tuple<bool, string>, IlTypeBuilder>();
		private const string DefaultAssemblyNameString = "Dapplo.InterfaceImpl.Generated";
		private static readonly string[] SpecialPrefixes = {"get_","set_","add_","remove_"};
		private readonly AssemblyBuilder _assemblyBuilder;
		private readonly ModuleBuilder _moduleBuilder;
		private readonly bool _allowSave;

		/// <summary>
		/// IlTypeBuilder factory, returns the DefaultInstance if there is no derrivate
		/// </summary>
		/// <param name="allowSave">specify true if you also want to be able to save</param>
		/// <param name="assemblyNameString">Name of the assembly</param>
		public static IlTypeBuilder CreateOrGet(bool allowSave = false, string assemblyNameString = DefaultAssemblyNameString)
		{
			lock (IlTypeBuilderCache)
			{
				var key = new Tuple<bool, string>(allowSave, assemblyNameString);
				IlTypeBuilder result;
				if (!IlTypeBuilderCache.TryGetValue(key, out result))
				{
					result = new IlTypeBuilder(allowSave, assemblyNameString);
					IlTypeBuilderCache.Add(key, result);
				}
				return result;
			}
		}

		/// <summary>
		/// Create an IlTypeBuilder
		/// </summary>
		/// <param name="allowSave">specify true if you also want to be able to save</param>
		/// <param name="assemblyNameString">Name of the assembly</param>
		private IlTypeBuilder(bool allowSave = false, string assemblyNameString = DefaultAssemblyNameString)
		{
			_allowSave = allowSave;
			string dllName = $"{assemblyNameString}.dll";
			var assemblyName = new AssemblyName(assemblyNameString);
			var appDomain = AppDomain.CurrentDomain;
			_assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, allowSave ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.RunAndCollect, appDomain.BaseDirectory);
			_moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName.Name, dllName, false);
		}


		/// <summary>
		/// Checks if this type builder already created the type, or return null if not
		/// </summary>
		/// <param name="fqTypeName">The name of the type</param>
		/// <param name="type">The out variable for the type</param>
		/// <returns>bool with true if found</returns>
		public bool TryGetType(string fqTypeName, out Type type)
		{

			type = _assemblyBuilder.GetTypes().FirstOrDefault(x => x.FullName == fqTypeName);
			if (type == null)
			{
				type = Type.GetType(fqTypeName);
				if (type != null)
				{
					Log.Verbose().WriteLine("Found Type for {0}", fqTypeName);
				}
			}
			else
			{
				Log.Verbose().WriteLine("Found cached instance of {0}", fqTypeName);
			}

			return type != null;
		}

		/// <summary>
		///     Creates an implementation as Type for a given interface, which can be intercepted
		/// </summary>
		/// <param name="typeName">Name of the type to generate</param>
		/// <param name="implementingInterfaces">Interfaces to implement</param>
		/// <param name="baseType">Type as base</param>
		/// <returns>Type</returns>
		public Type CreateType(string typeName, Type[] implementingInterfaces, Type baseType = null)
		{
			Log.Verbose().WriteLine("Creating type {0}", typeName);

			// Make sure to return the type from "cache" if possible
			Type cachedType;
			if (TryGetType(typeName, out cachedType))
			{
				return cachedType;
			}

			// The base type always have a value, normally everything extends object
			if (baseType == null)
			{
				baseType = typeof(object);
			}

			// Create the type, and let it implement our interface
			var typeBuilder = _moduleBuilder.DefineType(typeName,
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
				baseType, implementingInterfaces);

			// Make a collection of already implemented properties
			var baseProperties = baseType.GetRuntimeProperties().Select(x => x.Name).ToList();

			var propertyInfos =
				from iface in implementingInterfaces
				from propertyInfo in iface.GetProperties()
				select propertyInfo;

			var processedProperties = new Dictionary<string, Type>();
			foreach (var propertyInfo in propertyInfos)
			{
				if (processedProperties.ContainsKey(propertyInfo.Name))
				{
					continue;
				}
				if (baseProperties.Contains(propertyInfo.Name))
				{
					continue;
				}
				if (!propertyInfo.CanRead && !propertyInfo.CanWrite)
				{
					continue;
				}

				// Create get and/or set
				IlGetSetBuilder.BuildGetSet(typeBuilder, propertyInfo);

				processedProperties.Add(propertyInfo.Name, propertyInfo.DeclaringType);
			}

			// Make a collection of already implemented method
			var baseMethods = baseType.GetRuntimeMethods().Select(x => x.Name).ToList();

			var methodInfos =
				from iface in implementingInterfaces
				from methodInfo in iface.GetMethods()
				select methodInfo;

			foreach (var methodInfo in methodInfos)
			{
				if (baseMethods.Contains(methodInfo.Name))
				{
					continue;
				}
				if (SpecialPrefixes.Any(x => methodInfo.Name.StartsWith(x)))
				{
					continue;
				}
				IlMethodBuilder.BuildMethod(typeBuilder, methodInfo);
			}

			// Now generate the methods for the event redirections
			var eventInfos =
				from iface in implementingInterfaces
				from eventInfo in iface.GetEvents()
				select eventInfo;
			foreach (var eventInfo in eventInfos)
			{
				IlEventBuilder.BuildEvent(typeBuilder, eventInfo, baseMethods);
			}

			Log.Verbose().WriteLine("Created type {0}", typeName);
			return typeBuilder.CreateType();
		}

		/// <summary>
		///     Save the "up to now" generated assembly
		/// </summary>
		/// <param name="dllName">Full path for the DLL</param>
		public void SaveAssemblyDll(string dllName)
		{
			if (!_allowSave)
			{
				throw new InvalidOperationException("Only allowed when before generation types the AllowSave was set to true.");
			}
			_assemblyBuilder.Save(dllName, PortableExecutableKinds.ILOnly, ImageFileMachine.AMD64);
			Log.Debug().WriteLine("Wrote {0} to {1}", dllName, AppDomain.CurrentDomain.BaseDirectory);
		}
	}
}