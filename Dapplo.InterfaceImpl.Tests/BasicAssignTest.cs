﻿//  Dapplo - building blocks for desktop applications
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

using Dapplo.InterfaceImpl.Tests.Interfaces;
using Dapplo.Log.XUnit;
using Dapplo.Log.Facade;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.InterfaceImpl.Tests
{
	public class BassicAssignTest
	{
		private readonly IBassicAssignTest _bassicAssignTest;

		public BassicAssignTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_bassicAssignTest = InterceptorFactory.New<IBassicAssignTest>();
		}

		[Fact]
		public void TestAssign()
		{
			const string testValue = "Robin";
			_bassicAssignTest.Name = testValue;
			Assert.Equal(testValue, _bassicAssignTest.Name);
		}
	}
}