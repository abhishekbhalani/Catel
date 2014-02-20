﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelServiceBaseTest.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Test.Services
{
    using Catel.Services;

#if NETFX_CORE
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

    [TestClass]
    public class ViewModelServiceBaseTest
    {
        #region Classes
        private class ViewModelService : ViewModelServiceBase
        {
        }
        #endregion

        #region Methods
        [TestMethod]
        public void Name()
        {
            var testService = new ViewModelService();

            Assert.AreEqual("ViewModelService", testService.Name);
        }
        #endregion
    }
}