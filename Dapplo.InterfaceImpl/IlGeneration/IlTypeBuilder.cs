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
using Dapplo.LogFacade;

#endregion

namespace Dapplo.InterfaceImpl.IlGeneration
{
	/// <summary>
	///     Internally used to generate a method via IL
	/// </summary>
	public class IlTypeBuilder
	{
		private static readonly LogSource Log = new LogSource();
		private const string DefaultAssemblyNameString = "Dapplo.InterfaceImpl.Generated";
		private readonly AssemblyBuilder _assemblyBuilder;
		private readonly ModuleBuilder _moduleBuilder;
		private readonly bool _allowSave;

		/// <summary>
		/// Create an IlTypeBuilder
		/// </summary>
		/// <param name="allowSave">specify true if you also want to be able to save</param>
		/// <param name="assemblyNameString">Name of the assembly</param>
		public IlTypeBuilder(bool allowSave = false, string assemblyNameString = DefaultAssemblyNameString)
		{
			_allowSave = allowSave;
			string dllName = $"{assemblyNameString}.dll";
			var assemblyName = new AssemblyName(assemblyNameString);
			var appDomain = AppDomain.CurrentDomain;
			_assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, allowSave ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.RunAndCollect, appDomain.BaseDirectory);
			_moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName.Name, dllName, false);
		}

		/// <summary>
		///     Creates an implementation as Type for a given interface, which can be intercepted
		/// </summary>
		/// <param name="typeName">Name of the type to generate</param>
		/// <param name="implementingInterfaces">Interfaces to implement</param>
		/// <param name="baseType">Type as base</param>
		/// <returns>Type</returns>
		public Type CreateType(string typeName, Type[] implementingInterfaces, Type baseType)
		{
			Log.Verbose().WriteLine("Creating type {0}", typeName);

			// Create the type, and let it implement our interface
			var typeBuilder = _moduleBuilder.DefineType(typeName,
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
				baseType, implementingInterfaces);

			// Make a collection of already implemented properties
			var baseProperties = baseType.GetRuntimeProperties().Select(x => x.Name);

			var propertyInfos =
				from iface in implementingInterfaces
				from propertyInfo in iface.GetProperties()
				where !baseProperties.Contains(propertyInfo.Name)
				select propertyInfo;

			foreach (var propertyInfo in propertyInfos)
			{
				if (!propertyInfo.CanRead && !propertyInfo.CanWrite)
				{
					Log.Verbose().WriteLine("Skipping property {0}", propertyInfo.Name);
					continue;
				}

				// Create get and/or set
				IlGetSetBuilder.BuildGetSet(typeBuilder, propertyInfo);
			}

			// Make a collection of already implemented method
			var baseMethods = baseType.GetRuntimeMethods().Select(x => x.Name);

			var methodInfos =
				from iface in implementingInterfaces
				from methodInfo in iface.GetMethods()
				where !baseMethods.Contains(methodInfo.Name)
				select methodInfo;

			foreach (var methodInfo in methodInfos)
			{
				if (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"))
				{
					Log.Verbose().WriteLine("Skipping method {0}", methodInfo.Name);
					continue;
				}
				IlMethodBuilder.BuildMethod(typeBuilder, methodInfo);
				Log.Verbose().WriteLine("Created method {0}", methodInfo.Name);
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