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

using System.Reflection;

#endregion

namespace Dapplo.InterfaceImpl.Extensions
{
	/// <summary>
	///     Base class for extensions, this should take away some default handling
	/// </summary>
	public abstract class AbstractInterceptorExtension : IInterceptorExtension
	{
		/// <summary>
		///     Initialize the extension, e.g. register methods etc.
		/// </summary>
		/// <param name="interceptor"></param>
		public virtual void Initialize(IExtensibleInterceptor interceptor)
		{
		}

		/// <summary>
		///     This returns 0, which means somewhere in the middle
		///     If an extension needs to be called last, it should override this and for example return int.MaxValue
		///     If an extension needs to be called first, it should override this and for example return int.MinValue
		/// </summary>
		public virtual int InitOrder => 0;

		/// <summary>
		///     Handle every property
		/// </summary>
		/// <param name="interceptor"></param>
		/// <param name="propertyInfo"></param>
		public virtual void InitProperty(IExtensibleInterceptor interceptor, PropertyInfo propertyInfo)
		{
		}

		/// <summary>
		///     After property initialization
		/// </summary>
		/// <param name="interceptor"></param>
		public virtual void AfterInitialization(IExtensibleInterceptor interceptor)
		{
		}
	}
}