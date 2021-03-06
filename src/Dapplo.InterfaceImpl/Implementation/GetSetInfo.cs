//  Dapplo - building blocks for desktop applications
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

#endregion

namespace Dapplo.InterfaceImpl.Implementation
{
	/// <summary>
	/// Information for a Get or Set invocation
	/// </summary>
	public class GetSetInfo
	{
		/// <summary>
		/// The interceptor that created this GetSetInfo
		/// </summary>
		public IExtensibleInterceptor Interceptor { get; set; }

		/// <summary>
		///     Can the proxy continue with other getter/setters?
		///     This should be set to false if a getter/setter implementation wants to throw an exception or thinks there should be
		///     no more others.
		/// </summary>
		public bool CanContinue { get; set; }

		/// <summary>
		///     This can be set to an exception if a getter/setter wants to throw an exception
		/// </summary>
		public Exception Error { get; set; }

		/// <summary>
		///     Simple property to check for error
		/// </summary>
		public bool HasError => Error != null;

		/// <summary>
		///     Property name of the property that is being get/set
		/// </summary>
		public string PropertyName { get; set; }

		/// <summary>
		///     Type of the property that is being get/set
		/// </summary>
		public Type PropertyType { get; set; }
	}
}