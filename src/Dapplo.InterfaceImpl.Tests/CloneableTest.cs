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

using Dapplo.InterfaceImpl.Tests.Interfaces;
using Dapplo.Log.XUnit;
using Dapplo.Log;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.InterfaceImpl.Tests
{
	/// <summary>
	/// Tests for the IShallowCloneable extension
	/// </summary>
	public class CloneableTest
	{
		private readonly ICloneableTest _cloneableTest;

		public CloneableTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_cloneableTest = InterceptorFactory.New<ICloneableTest>();
		}

		[Fact]
		public void TestClone()
		{
			const string testValue = "Robin";
			_cloneableTest.Name = testValue;
			var cloned = _cloneableTest.ShallowClone();
			Assert.Equal(testValue, cloned.Name);
			cloned.Name = "Dapplo";
			// The old instance should still have the previous value
			Assert.Equal(testValue, _cloneableTest.Name);
			// The cloned instance should NOT have the previous value
			Assert.NotEqual(testValue, cloned.Name);
		}
	}
}