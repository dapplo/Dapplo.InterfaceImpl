﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2017 Dapplo
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Dapplo.Log;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

#endregion

namespace Dapplo.InterfaceImpl.Implementation
{
	/// <summary>
	///     Implementation of the IInterceptor
	/// </summary>
	public class ExtensibleInterceptorImpl<T> : IExtensibleInterceptor
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly LogSource Log = new LogSource();
		// ReSharper disable once StaticMemberInGenericType
		private static readonly AbcComparer AbcComparerInstance = new AbcComparer();

		private readonly IList<IInterceptorExtension> _extensions = new List<IInterceptorExtension>();
		private readonly IList<Getter> _getters = new List<Getter>();
		private readonly IDictionary<string, IList<Action<MethodCallInfo>>> _methodMap = new Dictionary<string, IList<Action<MethodCallInfo>>>();
		private IDictionary<string, object> _properties = new Dictionary<string, object>(AbcComparerInstance);
		private readonly IList<Setter> _setters = new List<Setter>();

		/// <summary>
		///     Constructor
		/// </summary>
		public ExtensibleInterceptorImpl()
		{
			// Make sure the default set logic is registered
			RegisterSetter((int) CallOrder.Middle, DefaultSet);
			// Make sure the default get logic is registered
			RegisterGetter((int) CallOrder.Middle, DefaultGet);
		}


		/// <summary>
		///     Add an extension to the proxy, these extensions contain logic which enhances the proxy
		/// </summary>
		/// <param name="extensionType">Type for the extension</param>
		public void AddExtension(Type extensionType)
		{
			if (extensionType.IsGenericType)
			{
				extensionType = extensionType.MakeGenericType(typeof (T));
			}

			var extension = (IInterceptorExtension) Activator.CreateInstance(extensionType);
			extension.Initialize(this);
			_extensions.Add(extension);
		}

		/// <summary>
		///     Get the description attribute for a property
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>description</returns>
		public string Description(string propertyName)
		{
			var proxiedType = typeof (T);
			var propertyInfo = proxiedType.GetProperty(propertyName);
			return propertyInfo.GetDescription();
		}

		/// <summary>
		///     Initialize, make sure every property is processed by the extensions
		/// </summary>
		public virtual void Init()
		{
			// Init in the right order
			var extensions = (from sortedExtension in _extensions
				orderby sortedExtension.InitOrder ascending
				select sortedExtension).ToList();

			// Exclude properties from this assembly
			var thisAssembly = GetType().Assembly;

			// as GetInterfaces doesn't return the type itself (makes sense), the following 2 lines makes a list of all
			var interfacesToCheck = new[] { typeof(T) }.Concat(typeof(T).GetInterfaces()).ToList();

			var propertyTypes = new Dictionary<string, Type>(AbcComparerInstance);
			PropertyTypes = new ReadOnlyDictionary<string, Type>(propertyTypes);

			// Now, create an IEnumerable for all the property info of all the properties in the interfaces that the
			// "user" code introduced in the type. (e.g skip all types & properties from this assembly)
			var allPropertyInfos = (from interfaceType in interfacesToCheck
				where interfaceType.Assembly != thisAssembly
				from propertyInfo in interfaceType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
				where propertyInfo.GetIndexParameters().Length == 0
				select propertyInfo).GroupBy(p => p.Name).Select(group => group.First());

			foreach (var propertyInfo in allPropertyInfos)
			{
				propertyTypes.Add(propertyInfo.Name, propertyInfo.PropertyType);
				InitProperty(propertyInfo, extensions);
			}

			AfterInitialization(extensions);

			// Throw if an exception was left over
			if (InitializationErrors.Count > 0)
			{
				throw InitializationErrors.Values.First();
			}
		}

		/// <summary>
		/// Called after the Initialization, this allows us to e.g. ignore errors
		/// </summary>
		/// <param name="extensions"></param>
		protected virtual void AfterInitialization(IEnumerable<IInterceptorExtension> extensions)
		{
			// Call all AfterInitialization, this allows us to ignore errors
			foreach (var extension in extensions)
			{
				extension.AfterInitialization(this);
			}
		}

		/// <summary>
		/// Call all extensions to initialize whatever needs to be initialized for a property
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <param name="extensions">IList with IInterceptorExtension</param>
		protected virtual void InitProperty(PropertyInfo propertyInfo, IEnumerable<IInterceptorExtension> extensions)
		{
			foreach (var extension in extensions)
			{
				try
				{
					extension.InitProperty(this, propertyInfo);
				}
				catch (Exception ex)
				{
					Log.Warn().WriteLine(ex.Message);
					InitializationErrors[propertyInfo.Name] = ex;
				}
			}
		}

		#region IInterceptor

		/// <summary>
		/// Make a shallow copy of the instance
		/// </summary>
		/// <returns>new instance with only the references copied</returns>
		object ICloneable.Clone()
		{
			var clonedObject = (ExtensibleInterceptorImpl<T>)MemberwiseClone();

			// Normally a ShallowClone is not so hard, but here we would have a really issue as the values
			// are inside a dictionary, and copying the reference would NOT be enough
			// This makes sure the reference to the backing properties is not a simple copy
			clonedObject._properties = new Dictionary<string, object>(_properties, AbcComparerInstance);

			// Make sure all event handlers are removed, to prevent memory leaks and weird behaviors
			clonedObject.RemoveEventHandlers();
			return clonedObject;
		}

		/// <summary>
		///     If an exception is catched during the initialization, it can be found here
		/// </summary>
		public IDictionary<string, Exception> InitializationErrors { get; } = new Dictionary<string, Exception>(AbcComparerInstance);

		/// <summary>
		///     Get type of a property
		/// </summary>
		public IReadOnlyDictionary<string, Type> PropertyTypes { get; private set; }

		/// <summary>
		///     Get the raw property values of the property object
		///     Can be used to modify the directly, or for load/save
		///     Assignment to this will copy all the supplied properties.
		/// </summary>
		public IDictionary<string, object> Properties
		{
			get { return _properties; }
			set
			{
				foreach (var key in value.Keys)
				{
					_properties[key] = value[key];
				}
			}
		}

		/// <summary>
		/// The type which is intercepted
		/// </summary>
		public Type InterceptedType => typeof (T);

		/// <summary>
		///     Get the value for a property.
		/// Note: This needs to be virtual otherwise the interface isn't implemented
		/// </summary>
		/// <param name="key">string with key for the property to get</param>
		/// <returns>object or null if not available</returns>
		public virtual object this[string key]
		{
			get
			{
				Properties.TryGetValue(key, out var value);
				return value;
			}
		}

		/// <summary>
		/// Retrieve the real Property name, if you only have a key whích isn't exact
		/// </summary>
		/// <param name="possibleProperyName">Possible property name, like without capitals etc</param>
		/// <returns>The real property name</returns>
		public string PropertyNameFor(string possibleProperyName)
		{
			return PropertyTypes.Keys.FirstOrDefault(x => AbcComparerInstance.Compare(x, possibleProperyName) == 0);
		}


		/// <summary>
		///     Register a method for the proxy
		/// </summary>
		/// <param name="methodname">string</param>
		/// <param name="methodAction">Action which accepts a MethodCallInfo</param>
		public void RegisterMethod(string methodname, Action<MethodCallInfo> methodAction)
		{
			if (!_methodMap.TryGetValue(methodname, out var functions))
			{
				functions = new List<Action<MethodCallInfo>>();
				_methodMap.Add(methodname, functions);
			}
			functions.Add(methodAction);
		}

		/// <summary>
		///     Register a setter, this will be called when the proxy's set is called.
		/// </summary>
		/// <param name="order">int to specify when (in what order) the setter is called</param>
		/// <param name="setterAction">Action which accepts SetInfo</param>
		public void RegisterSetter(int order, Action<SetInfo> setterAction)
		{
			_setters.Add(new Setter
			{
				Order = order,
				SetterAction = setterAction
			});
			((List<Setter>) _setters).Sort();
		}

		/// <summary>
		///     Register a getter, this will be called when the proxy's get is called.
		/// </summary>
		/// <param name="order">int to specify when (in what order) the getter is called</param>
		/// <param name="getterAction">Action which accepts GetInfo</param>
		public void RegisterGetter(int order, Action<GetInfo> getterAction)
		{
			_getters.Add(new Getter
			{
				Order = order,
				GetterAction = getterAction
			});
			((List<Getter>) _getters).Sort();
		}

		#endregion

		#region Intercepting code

		/// <summary>
		///     Pretend the get on the property object was called
		///     This will invoke the normal get, going through all the registered getters
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		public GetInfo Get(string propertyName)
		{
			if (!PropertyTypes.TryGetValue(propertyName, out var propertyType))
			{
				propertyType = typeof(object);
			}

			var hasValue = _properties.TryGetValue(propertyName, out var value);
			var getInfo = new GetInfo
			{
				Interceptor = this,
				PropertyName = propertyName,
				PropertyType = propertyType,
				CanContinue = true,
				Value = value,
				HasValue = hasValue
			};

			foreach (var getter in _getters)
			{
				getter.GetterAction(getInfo);
				if (!getInfo.CanContinue || getInfo.Error != null)
				{
					break;
				}
			}
			return getInfo;
		}

		/// <summary>
		///     Pretend the set on the property object was called
		///     This will invoke the normal set, going through all the registered setters
		/// </summary>
		/// <param name="propertyName">Name of the property to set</param>
		/// <param name="value">Value to set</param>
		public void Set(string propertyName, object value)
		{
			var propertyInfo = PropertyTypes[propertyName];

			var hasOldValue = _properties.TryGetValue(propertyName, out var oldValue);
			var setInfo = new SetInfo
			{
				Interceptor = this,
				PropertyName = propertyName,
				PropertyType = propertyInfo,
				CanContinue = true,
				NewValue = value,
				HasOldValue = hasOldValue,
				OldValue = oldValue
			};

			foreach (var setter in _setters)
			{
				setter.SetterAction(setInfo);
				if (!setInfo.CanContinue || setInfo.Error != null)
				{
					break;
				}
			}
			if (setInfo.HasError && setInfo.Error != null)
			{
				throw setInfo.Error;
			}
		}

		/// <summary>
		///     The method invocation
		/// </summary>
		/// <param name="methodName">string</param>
		/// <param name="parameters">params</param>
		/// <returns>return value</returns>
		public object Invoke(string methodName, params object[] parameters)
		{
			// First check the methods, so we can override all other access by specifying a method
			if (!_methodMap.TryGetValue(methodName, out var actions))
			{
				throw new NotImplementedException();
			}
			var methodCallInfo = new MethodCallInfo
			{
				Interceptor = this,
				MethodName = methodName,
				Arguments = parameters
			};
			foreach (var action in actions)
			{
				action(methodCallInfo);
				if (methodCallInfo.HasError)
				{
					break;
				}
			}
			if (methodCallInfo.HasError)
			{
				throw methodCallInfo.Error;
			}
			// TODO: make out parameters possible
			return methodCallInfo.ReturnValue;
		}

		#endregion

		#region Default Get/Set

		/// <summary>
		///     A default implementation of the get logic
		/// </summary>
		/// <param name="getInfo">GetInfo</param>
		private void DefaultGet(GetInfo getInfo)
		{
			if (getInfo.PropertyName == null)
			{
				getInfo.HasValue = false;
				return;
			}
			if (getInfo.Interceptor.Properties.TryGetValue(getInfo.PropertyName, out var value))
			{
				getInfo.Value = value;
				getInfo.HasValue = true;
			}
			else
			{
				// Make sure we return the right default value, when passed by-ref there needs to be a value
				if (!PropertyTypes.TryGetValue(getInfo.PropertyName, out var propertyType))
				{
					propertyType = typeof(object);
				}
				getInfo.Value = propertyType.CreateInstance();
				getInfo.HasValue = false;
			}
		}

		/// <summary>
		///     A default implementation of the set logic
		/// </summary>
		/// <param name="setInfo">SetInfo</param>
		private void DefaultSet(SetInfo setInfo)
		{
			var propertyType = PropertyTypes[setInfo.PropertyName];

			var newValue = propertyType.ConvertOrCastValueToType(setInfo.NewValue);
			// Add the value to the dictionary
			setInfo.Interceptor.Properties[setInfo.PropertyName] = newValue;
		}

		#endregion
	}
}