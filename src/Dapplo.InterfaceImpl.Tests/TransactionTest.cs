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

using Dapplo.InterfaceImpl.Tests.Interfaces;
using Dapplo.Log.XUnit;
using Dapplo.Log;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.InterfaceImpl.Tests
{
	/// <summary>
	///     This test class tests the transactional capabilities of the proxy
	/// </summary>
	public class TransactionTest
	{
		private readonly ITransactionTest _transactionTest;

		public TransactionTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);

			_transactionTest = InterceptorFactory.New<ITransactionTest>();
		}

		[Fact]
		public void TestTransactionCommit()
		{
			var properties = _transactionTest;
			properties.Age = 30;
			properties.StartTransaction();
			Assert.Equal(30, properties.Age);
			Assert.False(properties.IsTransactionDirty());
			properties.Age = 35;
			Assert.True(properties.IsTransactionDirty());
			Assert.Equal(35, properties.Age);
			properties.CommitTransaction();
			Assert.Equal(35, properties.Age);
		}

		[Fact]
		public void TestTransactionRollback()
		{
			var properties = _transactionTest;
			properties.Age = 30;
			properties.StartTransaction();
			Assert.Equal(30, properties.Age);
			properties.Age = 35;
			Assert.Equal(35, properties.Age);
			properties.RollbackTransaction();
			Assert.Equal(30, properties.Age);
		}
	}
}