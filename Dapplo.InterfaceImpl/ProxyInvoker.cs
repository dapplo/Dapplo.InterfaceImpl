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

#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dapplo.LogFacade;

#endregion

namespace Dapplo.InterfaceImpl
{
	public class ProxyInvoker : IInterceptor
	{
		private static readonly LogSource Log = new LogSource();
		private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

		public void Get(GetInfo getInfo)
		{
			Log.Debug().WriteLine("Get {0}", getInfo.PropertyName);

			if (_values.ContainsKey(getInfo.PropertyName))
			{
				getInfo.Value = _values[getInfo.PropertyName];
			}
			if (getInfo.PropertyType.IsValueType)
			{
				getInfo.Value = Activator.CreateInstance(getInfo.PropertyType);
			}
		}

		public void Set(SetInfo setInfo)
		{
			Log.Debug().WriteLine("Set {0} => {1}", setInfo.PropertyName, setInfo.NewValue);

			if (_values.ContainsKey(setInfo.PropertyName))
			{
				_values[setInfo.PropertyName] = setInfo.NewValue;
			}
			else
			{
				_values.Add(setInfo.PropertyName, setInfo.NewValue);
			}
		}

		public object Invoke(string methodName, params object[] parameters)
		{
			Debug.WriteLine("{0}({1})", methodName, string.Join(",", parameters));
			//throw new Exception("Blub");
			return "ThisIsIgnored";
		}
	}
}