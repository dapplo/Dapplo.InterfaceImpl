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
using System.Linq.Expressions;

#endregion

namespace Dapplo.InterfaceImpl.Extensions
{
	/// <summary>
	///     Attribute to "Tag" properties as with certain information
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TagAttribute : Attribute
	{
		/// <summary>
		/// Constructor for the TagAttribute
		/// </summary>
		/// <param name="tag">object with value for the tag</param>
		public TagAttribute(object tag)
		{
			Tag = tag;
		}

		/// <summary>
		/// Constructor for the TagAttribute
		/// </summary>
		/// <param name="tag">object with value for the tag</param>
		/// <param name="tagValue">object with value for the tag value</param>
		public TagAttribute(object tag, object tagValue) : this(tag)
		{
			TagValue = tagValue;
		}

		/// <summary>
		/// The tag
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Get (or set) the value of the tag
		/// </summary>
		public object TagValue { get; set; }
	}

	/// <summary>
	///     Interface which your interface needs to implement to be able to see if a property is tagged
	/// </summary>
	public interface ITagging
	{
		/// <summary>
		///     Retrieve the value for tag
		/// </summary>
		/// <param name="propertyName">Name of the property to get the tag value</param>
		/// <param name="tag">The tag value to get</param>
		/// <returns>Tagged value or null</returns>
		object GetTagValue(string propertyName, object tag);

		/// <summary>
		///     Checks if the supplied expression resolves to a property which has the expert attribute
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>true if the property has the expert attribute, else false</returns>
		bool IsTaggedWith(string propertyName, object tag);
	}

	/// <summary>
	///     Interface which your interface needs to implement to be able to see if a property is tagged
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ITagging<T> : ITagging
	{
		/// <summary>
		///     Retrieve the value for tag
		/// </summary>
		/// <typeparam name="TProp">Your interfaces</typeparam>
		/// <param name="propertyExpression"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>Tagged value or null</returns>
		object GetTagValue<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag);

		/// <summary>
		///     Checks if the supplied expression resolves to a property which has the expert attribute
		/// </summary>
		/// <typeparam name="TProp">Your interfaces</typeparam>
		/// <param name="propertyExpression"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>true if the property has the expert attribute, else false</returns>
		bool IsTaggedWith<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag);
	}
}