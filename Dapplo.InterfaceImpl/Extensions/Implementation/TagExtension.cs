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
using System.Collections.Generic;
using System.Reflection;
using Dapplo.InterfaceImpl.Implementation;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

#endregion

namespace Dapplo.InterfaceImpl.Extensions.Implementation
{
	[Extension(typeof (ITagging))]
	internal class TagExtension : AbstractInterceptorExtension
	{
		// The set of found expert properties
		private readonly IDictionary<string, IDictionary<object, object>> _taggedProperties = new Dictionary<string, IDictionary<object, object>>(new AbcComparer());

		/// <summary>
		///     Check if a property is tagged with a certain tag
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void GetTagValue(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = false;
			IDictionary<object, object> tags;
			if (!_taggedProperties.TryGetValue(methodCallInfo.PropertyNameOf(0), out tags))
			{
				return;
			}
			var hasTag = tags.ContainsKey(methodCallInfo.Arguments[1]);
			object returnValue = null;
			if (hasTag)
			{
				returnValue = tags[methodCallInfo.Arguments[1]];
			}
			methodCallInfo.ReturnValue = returnValue;
		}

		/// <summary>
		///     Register methods
		/// </summary>
		/// <param name="interceptor"></param>
		public override void Initialize(IExtensibleInterceptor interceptor)
		{
			base.Initialize(interceptor);

			// Use Lambda to make refactoring possible, this registers one method and the overloading is handled in the IsTaggedWith
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<ITagging>(x => x.IsTaggedWith("", null)), IsTaggedWith);
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<ITagging>(x => x.GetTagValue("", null)), GetTagValue);
		}

		/// <summary>
		///     Process the property, in our case get the tags
		/// </summary>
		/// <param name="interceptor"></param>
		/// <param name="propertyInfo"></param>
		public override void InitProperty(IExtensibleInterceptor interceptor, PropertyInfo propertyInfo)
		{
			var customAttributes = Attribute.GetCustomAttributes(propertyInfo);
			foreach (var customAttribute in customAttributes)
			{
				var tagAttribute = customAttribute as TagAttribute;
				if (tagAttribute == null)
				{
					continue;
				}
				IDictionary<object, object> tags;
				if (!_taggedProperties.TryGetValue(propertyInfo.Name, out tags))
				{
					tags = new Dictionary<object, object>();
					_taggedProperties.Add(propertyInfo.Name, tags);
				}
				tags[tagAttribute.Tag] = tagAttribute.TagValue;
			}
		}

		/// <summary>
		///     Check if a property is tagged with a certain tag
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void IsTaggedWith(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = false;
			IDictionary<object, object> tags;
			if (_taggedProperties.TryGetValue(methodCallInfo.PropertyNameOf(0), out tags))
			{
				methodCallInfo.ReturnValue = tags.ContainsKey(methodCallInfo.Arguments[1]);
			}
		}
	}
}