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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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

		/// <summary>
		/// Retrieve all changes, 
		/// </summary>
		/// <returns>ISet with the property values</returns>
		ISet<string> Changes();

		/// <summary>
		/// Test if a property has been changed since the last reset
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>bool</returns>
		bool IsChanged(string propertyName);
	}

	/// <summary>
	/// This is the generic version of IHasChanges, which supports property expressions
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IHasChanges<T> : IHasChanges
	{
		/// <summary>
		/// Generic version of IsChanged which supports property expressions
		/// </summary>
		/// <typeparam name="TProp">Expression which supplies the property name</typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>bool</returns>
		bool IsChanged<TProp>(Expression<Func<T, TProp>> propertyExpression);
	}
}