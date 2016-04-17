using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExplodingJelly.SprayGenerator;
//using Nvidia.TextureTools;

namespace ExplodingJelly.SprayGeneratorTests
{
    [TestClass]
    public class VTFGeneratorTests
    {
        #region No Fading, No Alpha
        [TestMethod]
        [DeploymentItem(@"TestImages\Input\256_Static_NoAlpha.jpg", "TestInputs")]
        public void StaticImage_NoFading_NoAlpha()
        {
            const string testInputFile = @"TestInputs\256_Static_NoAlpha.jpg";
            const string actualOutputFile = @"ActualOutputs\256_Static_NoAlpha.vtf";

            VTFGenerator generator = new VTFGenerator(testInputFile, actualOutputFile);
            generator.Process(false);

        }

        [TestMethod]
        [DeploymentItem(@"TestImages\Input\256_test_stripes.png", "TestInputs")]
        public void StaticImage_NoFading_NoAlpha_Stripes()
        {
            const string testInputFile = @"TestInputs\256_test_stripes.png";
            const string actualOutputFile = @"ActualOutputs\256_test_stripes.vtf";

            VTFGenerator generator = new VTFGenerator(testInputFile, actualOutputFile);
            generator.Process(false);
        }

        [TestMethod]
        [DeploymentItem(@"TestImages\Input\256_stripe_animated.gif", "TestInputs")]
        public void Animation_NoFading_NoAlpha_Stripes()
        {
            const string testInputFile = @"TestInputs\256_stripe_animated.gif";
            const string actualOutputFile = @"ActualOutputs\256_stripe_animated.vtf";

            VTFGenerator generator = new VTFGenerator(testInputFile, actualOutputFile);
            generator.Process(false);
        } 
        #endregion

        #region No Fading, Alpha
        [TestMethod]
        [DeploymentItem(@"TestImages\Input\256_Static_NoAlpha.jpg", "TestInputs")]
        public void StaticImage_NoFading_Alpha()
        {
            const string testInputFile = @"TestInputs\256_Static_NoAlpha.jpg";
            const string actualOutputFile = @"ActualOutputs\256_Static_Alpha.vtf";

            VTFGenerator generator = new VTFGenerator(testInputFile, actualOutputFile);
            generator.Process(true);

        }

        [TestMethod]
        [DeploymentItem(@"TestImages\Input\256_test_stripes.png", "TestInputs")]
        public void StaticImage_NoFading_Alpha_Stripes()
        {
            const string testInputFile = @"TestInputs\256_test_stripes.png";
            const string actualOutputFile = @"ActualOutputs\256_alpha_stripes.vtf";

            VTFGenerator generator = new VTFGenerator(testInputFile, actualOutputFile);
            generator.Process(true);
        }

        [TestMethod]
        [DeploymentItem(@"TestImages\Input\256_stripe_animated.gif", "TestInputs")]
        public void Animation_NoFading_Alpha_Stripes()
        {
            const string testInputFile = @"TestInputs\256_stripe_animated.gif";
            const string actualOutputFile = @"ActualOutputs\256_alpha_stripe_animated.vtf";

            VTFGenerator generator = new VTFGenerator(testInputFile, actualOutputFile);
            generator.Process(true);
        } 
        #endregion

        [TestMethod]
        [DeploymentItem(@"TestImages\Input\256_Static_NoAlpha.jpg", "TestInputs")]
        [DeploymentItem(@"TestImages\Input\256_test_stripes.png", "TestInputs")]
        public void StaticImage_Fading_NoAlpha()
        {
            const string smallInputFile = @"TestInputs\256_Static_NoAlpha.jpg";
            const string bigInputFile = @"TestInputs\256_test_stripes.png";
            const string actualOutputFile = @"ActualOutputs\256_Static_Fading_NoAlpha.vtf";

            VTFGenerator generator = new VTFGenerator(smallInputFile, bigInputFile, actualOutputFile);
            generator.Process(false);

        }

        [TestMethod]
        [DeploymentItem(@"TestImages\Input\256_test_stripes.png", "TestInputs")]
        [DeploymentItem(@"TestImages\Input\256_Static_NoAlpha.jpg", "TestInputs")]
        public void StaticImage_Fading_Alpha()
        {
            const string smallInputFile = @"TestInputs\256_Static_NoAlpha.jpg";
            const string bigInputFile = @"TestInputs\256_test_stripes.png";
            const string actualOutputFile = @"ActualOutputs\256_Static_Fading_Alpha.vtf";

            VTFGenerator generator = new VTFGenerator(smallInputFile, bigInputFile, actualOutputFile);
            generator.Process(true);
        }

        [TestMethod]
        [DeploymentItem(@"TestImages\Input\510x510.gif", "TestInputs")]
        public void WeirdSize()
        {
            const string testInputFile = @"TestInputs\510x510.gif";
            const string actualOutputFile = @"ActualOutputs\weirdsize.vtf";

            VTFGenerator generator = new VTFGenerator(testInputFile, actualOutputFile);
            generator.Process(false);

        }
    }
}
