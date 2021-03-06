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
using System.Collections.Generic;
using Dapplo.InterfaceImpl.Implementation;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

#endregion

namespace Dapplo.InterfaceImpl.Extensions.Implementation
{
	/// <summary>
	///     This implements logic to add write protect support to your proxied interface.
	/// </summary>
	[Extension(typeof (IWriteProtectProperties))]
	internal class WriteProtectExtension : AbstractInterceptorExtension
	{
		// A store for the values that are write protected
		private readonly ISet<string> _writeProtectedProperties = new HashSet<string>(new AbcComparer());
		private bool _isProtecting;

		/// <summary>
		///     DisableWriteProtect removes the write protection of the supplied property in the LambdaExpression
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void DisableWriteProtect(MethodCallInfo methodCallInfo)
		{
			_writeProtectedProperties.Remove(methodCallInfo.PropertyNameOf(0));
		}

		/// <summary>
		///     Register setter and methods
		/// </summary>
		/// <param name="interceptor"></param>
		public override void Initialize(IExtensibleInterceptor interceptor)
		{
			base.Initialize(interceptor);

			interceptor.RegisterSetter((int) CallOrder.First, WriteProtectSetter);

			// Use Lambdas to make refactoring possible
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IWriteProtectProperties>(x => x.StartWriteProtecting()), StartWriteProtecting);
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IWriteProtectProperties>(x => x.RemoveWriteProtection()), RemoveWriteProtection);
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IWriteProtectProperties>(x => x.StopWriteProtecting()), StopWriteProtecting);
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IWriteProtectProperties>(x => x.WriteProtect("")), WriteProtect);
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IWriteProtectProperties>(x => x.DisableWriteProtect("")), DisableWriteProtect);
			interceptor.RegisterMethod(ExpressionExtensions.GetMemberName<IWriteProtectProperties>(x => x.IsWriteProtected("")), IsWriteProtected);
		}

		/// <summary>
		///     IsWriteProtected logic checks if the supplied property Lambda expression is write protected.
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void IsWriteProtected(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = _writeProtectedProperties.Contains(methodCallInfo.PropertyNameOf(0));
		}

		/// <summary>
		///     After calling this, nothing is write protected
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void RemoveWriteProtection(MethodCallInfo methodCallInfo)
		{
			_isProtecting = false;
			_writeProtectedProperties.Clear();
		}

		/// <summary>
		///     After calling this, every set will be write protected
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void StartWriteProtecting(MethodCallInfo methodCallInfo)
		{
			_isProtecting = true;
		}

		/// <summary>
		///     Stop write protecting every property
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void StopWriteProtecting(MethodCallInfo methodCallInfo)
		{
			_isProtecting = false;
		}

		/// <summary>
		///     WriteProtect sets the write protection of the supplied property in the LambdaExpression
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void WriteProtect(MethodCallInfo methodCallInfo)
		{
			_writeProtectedProperties.Add(methodCallInfo.PropertyNameOf(0));
		}

		/// <summary>
		///     This is the implementation of the set logic
		/// </summary>
		/// <param name="setInfo">SetInfo with all the information on the set call</param>
		private void WriteProtectSetter(SetInfo setInfo)
		{
			if (_writeProtectedProperties.Contains(setInfo.PropertyName))
			{
				setInfo.CanContinue = false;
				setInfo.Error = new AccessViolationException($"Property {setInfo.PropertyName} is write protected");
			}
			else if (_isProtecting)
			{
				_writeProtectedProperties.Add(setInfo.PropertyName);
			}
		}
	}
}