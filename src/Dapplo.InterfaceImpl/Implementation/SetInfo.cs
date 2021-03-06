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

#endregion

namespace Dapplo.InterfaceImpl.Implementation
{
	/// <summary>
	///     This class contains all the information for the setter actions
	/// </summary>
	public class SetInfo : GetSetInfo
	{
		/// <summary>
		///     Does property have an old value?
		/// </summary>
		public bool HasOldValue { get; set; }

		/// <summary>
		///     The new value for the property
		/// </summary>
		public object NewValue { get; set; }

		/// <summary>
		///     The old value of the property, if any (see HasOldValue)
		/// </summary>
		public object OldValue { get; set; }
	}
}