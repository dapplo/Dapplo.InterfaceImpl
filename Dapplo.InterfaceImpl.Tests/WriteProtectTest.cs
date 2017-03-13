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
using Dapplo.InterfaceImpl.Tests.Interfaces;
using Dapplo.Log.XUnit;
using Dapplo.Log;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.InterfaceImpl.Tests
{
	/// <summary>
	///     This test class shows how the write protect works
	/// </summary>
	public class WriteProtectTest
	{
		private const string TestValue1 = "VALUE1";
		private readonly IWriteProtectTest _writeProtectTest;

		public WriteProtectTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_writeProtectTest = InterceptorFactory.New<IWriteProtectTest>();
		}

		[Fact]
		public void TestAccessViolation()
		{
			_writeProtectTest.WriteProtect(x => x.Name);
			Assert.True(_writeProtectTest.IsWriteProtected(x => x.Name));

			Assert.Throws<AccessViolationException>(() => _writeProtectTest.Name = TestValue1);
		}

		[Fact]
		public void TestDisableWriteProtect()
		{
			_writeProtectTest.StartWriteProtecting();
			_writeProtectTest.Age = 30;
			Assert.True(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));
			Assert.True(_writeProtectTest.IsWriteProtected(x => x.Age));
			_writeProtectTest.StopWriteProtecting();
			Assert.True(_writeProtectTest.IsWriteProtected(x => x.Age));
			_writeProtectTest.DisableWriteProtect(x => x.Age);
			Assert.False(_writeProtectTest.IsWriteProtected(x => x.Age));
		}

		[Fact]
		public void TestRemoveWriteProtect()
		{
			_writeProtectTest.StartWriteProtecting();
			_writeProtectTest.Age = 30;
			Assert.True(_writeProtectTest.IsWriteProtected(x => x.Age));

			_writeProtectTest.RemoveWriteProtection();
			Assert.False(_writeProtectTest.IsWriteProtected(x => x.Age));
		}

		[Fact]
		public void TestWriteProtect()
		{
			_writeProtectTest.StartWriteProtecting();
			_writeProtectTest.Age = 30;
			Assert.True(_writeProtectTest.IsWriteProtected(x => x.Age));
			_writeProtectTest.StopWriteProtecting();
			Assert.True(_writeProtectTest.IsWriteProtected(x => x.Age));
			_writeProtectTest.Name = TestValue1;
			Assert.False(_writeProtectTest.IsWriteProtected(x => x.Name));
		}
	}
}