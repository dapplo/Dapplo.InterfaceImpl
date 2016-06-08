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
using System.Collections.Generic;
using System.Linq;
using Dapplo.InterfaceImpl.Extensions.Implementation;
using Dapplo.InterfaceImpl.IlGeneration;
using Dapplo.InterfaceImpl.Implementation;
using Dapplo.Utils.Extensions;
using Dapplo.LogFacade;

#endregion

namespace Dapplo.InterfaceImpl
{
	/// <summary>
	///     This class is a factory which can create an implementation for an interface.
	///     It can "new" the implementation, add intercepting code and extensions.
	/// </summary>
	public class InterceptorFactory
	{
		private static readonly LogSource Log = new LogSource();
		private static readonly IList<Type> ExtensionTypes = new List<Type>();
		private static readonly IDictionary<Type, Type> TypeMap = new Dictionary<Type, Type>();
		private static readonly IDictionary<Type, Type> BaseTypeMap = new Dictionary<Type, Type>();
		private static readonly IDictionary<Type, Type[]> DefaultInterfacesMap = new Dictionary<Type, Type[]>();
		private static readonly IlTypeBuilder TypeBuilder = IlTypeBuilder.CreateOrGet();

		/// <summary>
		/// Register the known extensions
		/// </summary>
		static InterceptorFactory()
		{
			RegisterExtension(typeof (DefaultValueExtension<>));
			RegisterExtension(typeof (DescriptionExtension<>));
			RegisterExtension(typeof (HasChangesExtension));
			RegisterExtension(typeof (NotifyPropertyChangedExtension));
			RegisterExtension(typeof (NotifyPropertyChangingExtension));
			RegisterExtension(typeof (TagExtension));
			RegisterExtension(typeof (TransactionExtension));
			RegisterExtension(typeof (WriteProtectExtension));
			RegisterExtension(typeof (CloneableExtension<>));
		}

		/// <summary>
		///     This should be used to define the base type for the implementation of the interface
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <param name="baseType">should extend ExtensibleInterceptorImpl</param>
		public static void DefineBaseTypeForInterface(Type interfaceType, Type baseType)
		{
			BaseTypeMap.AddOrOverwrite(interfaceType, baseType);
		}

		/// <summary>
		///     This should be used to difine which default interfaces are added to the interfaces
		///     e.g. the IIniSection gets IDefaultValue and IHasChanges
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <param name="defaultInterfaces"></param>
		public static void DefineDefaultInterfaces(Type interfaceType, Type[] defaultInterfaces)
		{
			DefaultInterfacesMap.AddOrOverwrite(interfaceType, defaultInterfaces);
		}

		/// <summary>
		///     If there is an implementation for the interface available, register it here.
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <param name="implementation"></param>
		public static void DefineImplementationTypeForInterface(Type interfaceType, Type implementation)
		{
			lock (TypeMap)
			{
				TypeMap.AddOrOverwrite(interfaceType, implementation);
			}
		}

		/// <summary>
		///     Create an implementation, or reuse an existing, for an interface.
		///     Create an instance, add intercepting code, which implements a range of interfaces
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <returns>implementation</returns>
		public static TResult New<TResult>()
		{
			// create the intercepted object
			return (TResult)New(typeof(TResult));
		}

		/// <summary>
		///     Create an implementation, or reuse an existing, for an interface.
		///     Create an instance, add intercepting code, which implements a range of interfaces
		/// </summary>
		/// <param name="interfaceType">Type</param>
		/// <returns>implementation</returns>
		public static IExtensibleInterceptor New(Type interfaceType)
		{
			// create the intercepted object
			if (!interfaceType.IsVisible)
			{
				throw new ArgumentException("Internal types are not allowed.", interfaceType.Name);
			}
			if (!interfaceType.IsInterface)
			{
				throw new ArgumentException("Only interfaces are allowed.", nameof(interfaceType));
			}
			// GetInterfaces doesn't return the type itself, so we need to add it.
			var implementingInterfaces = new[] { interfaceType }.Concat(interfaceType.GetInterfaces()).ToList();

			var implementingAndDefaultInterfaces = new List<Type>();
			foreach (var implementingInterface in implementingInterfaces.ToList())
			{
				implementingAndDefaultInterfaces.Add(implementingInterface);
				Type[] defaultInterfaces;
				if (DefaultInterfacesMap.TryGetValue(implementingInterface, out defaultInterfaces))
				{
					implementingAndDefaultInterfaces.AddRange(defaultInterfaces);
				}
			}
			implementingInterfaces = implementingAndDefaultInterfaces.Distinct().ToList();

			// Create an implementation, or lookup
			Type implementingType;
			lock (TypeMap)
			{
				if (!TypeMap.TryGetValue(interfaceType, out implementingType))
				{
					int typeIndex = 0;
					// Build a name for the type
					var typeName = interfaceType.Name + "Impl{0}";
					// Remove "I" at the start
					if (typeName.StartsWith("I"))
					{
						typeName = typeName.Substring(1);
					}
					var fqTypeName = interfaceType.FullName.Replace(interfaceType.Name, string.Format(typeName, typeIndex));

					// Only create it if it was not already created via another way
					while (TypeBuilder.TryGetType(string.Format(fqTypeName, typeIndex), out implementingType))
					{
						// Break loop if the types are assignable
						if (interfaceType.IsAssignableFrom(implementingType))
						{
							Log.Verbose().WriteLine("Using cached, probably created elsewhere, type: {0}", fqTypeName);
							break;
						}
						Log.Verbose().WriteLine("Cached type {0} was not compatible with the interface, ignoring.", fqTypeName);
						fqTypeName = interfaceType.FullName.Replace(interfaceType.Name, string.Format(typeName, typeIndex++));
					}


					if (implementingType == null)
					{
						// Use this baseType if nothing is specified
						var baseType = typeof(ExtensibleInterceptorImpl<>);
						foreach (var implementingInterface in implementingInterfaces)
						{
							if (BaseTypeMap.ContainsKey(implementingInterface))
							{
								baseType = BaseTypeMap[implementingInterface];
								break;
							}
						}
						// Make sure we have a non generic type, by filling in the "blanks"
						if (baseType.IsGenericType)
						{
							baseType = baseType.MakeGenericType(interfaceType);
						}
						implementingType = TypeBuilder.CreateType(fqTypeName, implementingInterfaces.ToArray(), baseType);
					}

					// Register the implementation for the interface
					TypeMap.AddOrOverwrite(interfaceType, implementingType);
				}
			}

			if (!interfaceType.IsAssignableFrom(implementingType))
			{
				throw new InvalidOperationException($"{interfaceType.AssemblyQualifiedName} and {implementingType.AssemblyQualifiedName} are not compatible!?");
			}

			// Create an instance for the implementation
			var interceptor = Activator.CreateInstance(implementingType) as IExtensibleInterceptor;

			if (interceptor == null)
			{
				throw new ArgumentNullException(nameof(interceptor), "Internal error, the created type didn't implement IExtensibleInterceptor.");
			}

			var genericImplementingInterfaces = implementingInterfaces.Where(x => x.IsGenericType).Select(x => x.GetGenericTypeDefinition()).ToList();
			// Add the extensions
			foreach (var extensionType in ExtensionTypes)
			{
				var extensionAttributes = (ExtensionAttribute[]) extensionType.GetCustomAttributes(typeof (ExtensionAttribute), false);
				foreach (var extensionAttribute in extensionAttributes)
				{
					var implementing = extensionAttribute.Implementing;
					if (implementingInterfaces.Contains(implementing) || genericImplementingInterfaces.Contains(implementing))
					{
						interceptor.AddExtension(extensionType);
					}
				}
			}
			interceptor.Init();
			return interceptor;
		}

		/// <summary>
		///     Use this to register an Type for extension
		/// </summary>
		/// <param name="extensionType">Type</param>
		public static void RegisterExtension(Type extensionType)
		{
			ExtensionTypes.Add(extensionType);
		}
	}
}