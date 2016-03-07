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
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.InterfaceImpl. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

namespace Dapplo.InterfaceImpl
{
	/// <summary>
	/// To intercept interface calls, implement this and set your class to the IIntercepted
	/// </summary>
	public interface IInterceptor
	{
		/// <summary>
		///     This is called when a property get is used on the intercepted class.
		///     The Get method should set the getInfo.Value for the return value
		/// </summary>
		/// <param name="getInfo"></param>
		void Get(GetInfo getInfo);

		/// <summary>
		///     This is called when a property set is used on the intercepted class.
		///     The Set method should process setInfo.NewValue.
		/// </summary>
		/// <param name="setInfo"></param>
		void Set(SetInfo setInfo);
	}
}