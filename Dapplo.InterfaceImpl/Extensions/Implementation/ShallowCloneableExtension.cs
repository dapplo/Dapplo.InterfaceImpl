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

#region using

using Dapplo.InterfaceImpl.Implementation;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

#endregion

namespace Dapplo.InterfaceImpl.Extensions.Implementation
{
	/// <summary>
	///     This implements logic to add write protect support to your proxied interface.
	/// </summary>
	[Extension(typeof (IShallowCloneable<>)),Extension(typeof(IShallowCloneable))]
	internal class ShallowCloneableExtension<T> : AbstractInterceptorExtension
		where T : class
	{
		/// <summary>
		///     Register setter and methods
		/// </summary>
		/// <param name="interceptor"></param>
		public override void Initialize(IExtensibleInterceptor interceptor)
		{
			base.Initialize(interceptor);
			// Use Lambdas to make refactoring possible
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IShallowCloneable<T>>(x => x.ShallowClone()), Clone);
		}

		/// <summary>
		///     IsWriteProtected logic checks if the supplied property Lambda expression is write protected.
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void Clone(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = methodCallInfo.Interceptor.Clone();
		}
	}
}