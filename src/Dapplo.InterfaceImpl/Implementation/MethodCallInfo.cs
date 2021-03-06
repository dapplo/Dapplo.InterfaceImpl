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

#endregion

namespace Dapplo.InterfaceImpl.Implementation
{
	/// <summary>
	///     This container holds all the information that is needed for extending the proxy with a method call
	///     It contains the arguments, return value and out arguments for an invoke.
	/// </summary>
	public class MethodCallInfo
	{
		private object[] _outArgs;

		/// <summary>
		/// The interceptor that created this MethodCallInfo
		/// </summary>
		public IExtensibleInterceptor Interceptor { get; set; }

		/// <summary>
		///     the supplied arguments for the invoke
		/// </summary>
		public object[] Arguments { get; set; }

		/// <summary>
		///     Exception which will be passed to the invoking code
		/// </summary>
		public Exception Error { get; set; }

		/// <summary>
		///     Simple check for the exception
		/// </summary>
		public bool HasError => Error != null;

		/// <summary>
		///     Name of the invoked method
		/// </summary>
		public string MethodName { get; set; }

		/// <summary>
		///     get the Out-Arguments-count
		///     The ReturnMessage expects the outArgs/outArgsCount to have the return value in there too, this is why the get
		///     honors this.
		///     See:
		///     https://connect.microsoft.com/VisualStudio/feedback/details/752487/realproxy-invoke-method-throws-an-overflowexception-when-calling-endinvoke
		/// </summary>
		public int OutArgsCount => _outArgs?.Length + 1 ?? 0;

		/// <summary>
		///     get/set the Out-Arguments
		///     The ReturnMessage expects the outArgs/outArgsCount to have the return value in there too, this is why the get
		///     honors this.
		///     See:
		///     https://connect.microsoft.com/VisualStudio/feedback/details/752487/realproxy-invoke-method-throws-an-overflowexception-when-calling-endinvoke
		/// </summary>
		public object[] OutArguments
		{
			get
			{
				if (_outArgs == null)
				{
					return null;
				}
				var newOutArgs = new object[_outArgs.Length + 1];
				newOutArgs[0] = ReturnValue;
				for (var i = 0; i < _outArgs.Length; i++)
				{
					newOutArgs[i + 1] = _outArgs[i];
				}
				return newOutArgs;
			}
			set { _outArgs = value; }
		}

		/// <summary>
		///     Return value for the invoke
		/// </summary>
		public object ReturnValue { get; set; }
	}
}