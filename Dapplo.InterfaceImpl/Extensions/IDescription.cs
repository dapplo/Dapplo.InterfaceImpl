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
using System.Linq.Expressions;

#endregion

namespace Dapplo.InterfaceImpl.Extensions
{
	/// <summary>
	///     Extend your property interface with this, and you can read the DescriptionAttribute
	/// </summary>
	public interface IDescription
	{
		/// <summary>
		///     Return the description of the property
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>the description, null if none</returns>
		string DescriptionFor(string propertyName);
	}

	/// <summary>
	///     Extend your property interface with this, and you can read the DescriptionAttribute
	/// </summary>
	public interface IDescription<T> : IDescription
	{
		/// <summary>
		///     Return the description of the property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>the description, null if none</returns>
		string DescriptionFor<TProp>(Expression<Func<T, TProp>> propertyExpression);
	}
}