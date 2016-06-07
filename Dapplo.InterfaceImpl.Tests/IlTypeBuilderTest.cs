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

using System.IO;
using Dapplo.InterfaceImpl.IlGeneration;
using Dapplo.InterfaceImpl.Tests.Interfaces;
using Dapplo.InterfaceImpl.Tests.Logger;
using Dapplo.LogFacade;
using Xunit;
using Xunit.Abstractions;
using Dapplo.InterfaceImpl.Implementation;

#endregion

namespace Dapplo.InterfaceImpl.Tests
{
	public class IlTypeBuilderTest
	{
		public IlTypeBuilderTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);
		}

		[Fact]
		public void TestSaveAssembly()
		{
			var typeBuilder = IlTypeBuilder.CreateOrGet(true, "Generated.Dapplo.InterfaceImpl.Tests");

			typeBuilder.CreateType("SimpleTypeTest", new[] { typeof (ISimpleTypeTest) });
			var tmpFileName = "MySimpleType.dll";

			typeBuilder.SaveAssemblyDll(tmpFileName);
			Assert.True(File.Exists(tmpFileName));
			File.Delete(tmpFileName);
		}

		[Fact]
		public void TestCreateType_Double()
		{
			// Create via factory
			var impl = InterceptorFactory.New<ISimpleTypeTest>();

			var testInterfaceType = typeof(ISimpleTypeTest);

			var typeName = testInterfaceType.Name + "Impl";
			// Remove "I" at the start
			if (typeName.StartsWith("I"))
			{
				typeName = typeName.Substring(1);
			}

			var fqTypeName = testInterfaceType.FullName.Replace(testInterfaceType.Name, typeName);

			var baseType = typeof(ExtensibleInterceptorImpl<>);
			// Make sure we have a non generic type, by filling in the "blanks"
			if (baseType.IsGenericType)
			{
				baseType = baseType.MakeGenericType(testInterfaceType);
			}

			// Create self
			var typeBuilder = IlTypeBuilder.CreateOrGet();
			var createdType = typeBuilder.CreateType(fqTypeName, new[] { testInterfaceType });
		}
	}
}