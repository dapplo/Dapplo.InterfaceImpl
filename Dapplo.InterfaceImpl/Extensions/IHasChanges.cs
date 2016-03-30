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

namespace Dapplo.InterfaceImpl.Extensions
{
	/// <summary>
	///     Extending the to be property interface with this, adds a way of know if there were changes sind the last reset
	///     Is used internally in the IniConfig to detect if a write is needed
	/// </summary>
	public interface IHasChanges
	{
		/// <summary>
		///     Check if there are changes pending
		/// </summary>
		/// <returns>true when there are changes</returns>
		bool HasChanges();

		/// <summary>
		///     Reset the has changes flag
		/// </summary>
		void ResetHasChanges();
	}
}