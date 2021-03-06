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

using System.ComponentModel;
using Dapplo.InterfaceImpl.Tests.Interfaces;
using Dapplo.Log.XUnit;
using Dapplo.Log;
using Xunit;
using Xunit.Abstractions;
using System;
using Dapplo.Utils.Notify;

#endregion

namespace Dapplo.InterfaceImpl.Tests
{
	public class NotifyPropertyChangedTest
	{
		private static readonly LogSource Log = new LogSource();
		private const string NoChange = "NOCHANGE";
		private const string TestValue1 = "VALUE1";
		private const string TestValue2 = "VALUE2";
		private readonly INotifyPropertyChangedTest _notifyPropertyChangedTest;

		public NotifyPropertyChangedTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_notifyPropertyChangedTest = InterceptorFactory.New<INotifyPropertyChangedTest>();
		}

		/// <summary>
		/// Test with Dapplo.Utils EventObservable.From
		/// </summary>
		[Fact]
		public void TestNotifyPropertyChanged_EventObservable()
		{
			PropertyChangedEventArgs testValue = null;
			using (_notifyPropertyChangedTest.OnPropertyChanged().Subscribe(pce => testValue = pce))
			{
				_notifyPropertyChangedTest.Name = "Test";
				Assert.Equal("Name", testValue.PropertyName);
			}
		}

		[Fact]
		public void TestNotifyPropertyChanged()
		{
			string changedPropertyName = null;
			var propChanged = new PropertyChangedEventHandler((sender, eventArgs) =>
			{
				changedPropertyName = eventArgs.PropertyName;
				Log.Debug().WriteLine("Property change notification for {0}", eventArgs.PropertyName);
			});

			// Test event handler
			_notifyPropertyChangedTest.PropertyChanged += propChanged;
			_notifyPropertyChangedTest.Name = TestValue1;
			Assert.Equal("Name", changedPropertyName);

			// Ensure that if the value is the same, we don't get an event
			changedPropertyName = NoChange;
			_notifyPropertyChangedTest.Name = TestValue1;
			Assert.Equal(NoChange, changedPropertyName);

			// Test if event handler is unregistered
			_notifyPropertyChangedTest.PropertyChanged -= propChanged;
			changedPropertyName = NoChange;
			_notifyPropertyChangedTest.Name = TestValue2;
			Assert.Equal(NoChange, changedPropertyName);
		}
	}
}