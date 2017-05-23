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
    ///     Test case to show how the default value works
    /// </summary>
    public class DefaultValueTest
    {
        private readonly IDefaultValueTest _defaultValueTest;

        public DefaultValueTest(ITestOutputHelper testOutputHelper)
        {
            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            _defaultValueTest = InterceptorFactory.New<IDefaultValueTest>();
        }

        [Fact]
        public void TestDefaultValue()
        {
            Assert.Equal(21, _defaultValueTest.Age);
            Assert.Equal(3, _defaultValueTest.Ages.Count);
        }

        [Fact]
        public void TestDefaultValueOverwrite()
        {
            var defaultValueOverwriteTest = InterceptorFactory.New<IDefaultValueOverwriteTest>();
            Assert.Equal(42, defaultValueOverwriteTest.Age);
        }

        [Fact]
        public void TestDefaultValueAtrribute()
        {
            var defaultValue = _defaultValueTest.DefaultValueFor(x => x.Age);
            Assert.Equal(21, defaultValue);
            defaultValue = _defaultValueTest.DefaultValueFor("Age");
            Assert.Equal(21, defaultValue);
        }

        [Fact]
        public void TestRestoreToDefaultValue()
        {
            _defaultValueTest.Age = 22;
            Assert.Equal(22, _defaultValueTest.Age);
            _defaultValueTest.RestoreToDefault(x => x.Age);
            Assert.Equal(21, _defaultValueTest.Age);
        }

        [Fact]
        public void TestUriArrayDefaultValue()
        {
            Assert.Contains(new Uri("http://1.dapplo.net"), _defaultValueTest.MyUris);
        }

        [Fact]
        public void TestDefaultValueWithError()
        {
            // Used to be:
            //var ex = Assert.Throws<InvalidCastException>(() => ProxyBuilder.CreateProxy<IDefaultValueWithErrorTest>());
            // Now it should run without error
            InterceptorFactory.New<IDefaultValueWithErrorTest>();
        }
    }
}