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
using System.Reflection;
using Dapplo.InterfaceImpl.Implementation;
using Dapplo.Log;
using Dapplo.Utils.Extensions;

#endregion

namespace Dapplo.InterfaceImpl.Extensions.Implementation
{
	/// <summary>
	///     This implements logic to set the default values on your property interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (IDefaultValue))]
	internal class DefaultValueExtension<T> : AbstractInterceptorExtension
	{
		private readonly LogSource _log = new LogSource();

		/// <summary>
		///     Make sure this extension is initialized first
		/// </summary>
		public override int InitOrder => int.MinValue;

		/// <summary>
		///     Retrieve the default value, using the TypeConverter
		/// </summary>
		/// <param name="propertyInfo">Property to get the default value for</param>
		/// <returns>object with the type converted default value</returns>
		private static object GetConvertedDefaultValue(PropertyInfo propertyInfo)
		{
			var defaultValue = propertyInfo.GetDefaultValue();
			if (defaultValue != null)
			{
				var typeConverter = propertyInfo.GetTypeConverter();
				var targetType = propertyInfo.PropertyType;
				defaultValue = targetType.ConvertOrCastValueToType(defaultValue, typeConverter);
			}
			return defaultValue;
		}

		/// <summary>
		///     Return the default value for a property
		/// </summary>
		private static void GetDefaultValue(MethodCallInfo methodCallInfo)
		{
			var propertyInfo = typeof (T).GetProperty(methodCallInfo.PropertyNameOf(0));
			// Prevent ArgumentNullExceptions
			if (propertyInfo != null)
			{
				methodCallInfo.ReturnValue = GetConvertedDefaultValue(propertyInfo);
			}
		}

		/// <summary>
		///     Register the methods
		/// </summary>
		/// <param name="interceptor"></param>
		public override void Initialize(IExtensibleInterceptor interceptor)
		{
			base.Initialize(interceptor);
			// this registers one method and the overloading is handled in the GetDefaultValue
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IDefaultValue>(x => x.DefaultValueFor("")), GetDefaultValue);
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IDefaultValue>(x => x.RestoreToDefault("")), RestoreToDefault);
		}

		/// <summary>
		///     Process the property, in our case set the default
		/// </summary>
		/// <param name="interceptor"></param>
		/// <param name="propertyInfo"></param>
		public override void InitProperty(IExtensibleInterceptor interceptor, PropertyInfo propertyInfo)
		{
			RestoreToDefault(interceptor, propertyInfo, out var ex);
			if (ex != null)
			{
				throw ex;
			}
		}

		/// <summary>
		///     Return the default value for a property
		/// </summary>
		private void RestoreToDefault(MethodCallInfo methodCallInfo)
		{
			var propertyInfo = typeof (T).GetProperty(methodCallInfo.PropertyNameOf(0));
			// Prevent ArgumentNullExceptions
			if (propertyInfo == null)
			{
				return;
			}
			RestoreToDefault(methodCallInfo.Interceptor, propertyInfo, out var _);
		}

		/// <summary>
		///     Method to restore a property to its default
		/// </summary>
		/// <param name="interceptor">IExtensibleInterceptor responsible for the object</param>
		/// <param name="propertyInfo"></param>
		/// <param name="exception">out value to get an exception</param>
		private void RestoreToDefault(IExtensibleInterceptor interceptor, PropertyInfo propertyInfo, out Exception exception)
		{
			object defaultValue = null;
			exception = null;
			try
			{
				defaultValue = GetConvertedDefaultValue(propertyInfo);
			}
			catch (Exception ex)
			{
				_log.Warn().WriteLine(ex.Message);
				// Store the exception so it can be used
				exception = ex;
			}

			if (defaultValue != null)
			{
				interceptor.Set(propertyInfo.Name, defaultValue);
				return;
			}
			try
			{
				defaultValue = propertyInfo.PropertyType.CreateInstance();
				interceptor.Set(propertyInfo.Name, defaultValue);
			}
			catch (Exception ex)
			{
				// Ignore creating the default type, this might happen if there is no default constructor.
				_log.Warn().WriteLine(ex.Message);
			}
		}
	}
}