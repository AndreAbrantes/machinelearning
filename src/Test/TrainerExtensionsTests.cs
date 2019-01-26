﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ML.Auto.Test
{
    [TestClass]
    public class TrainerExtensionsTests
    {
        [TestMethod]
        public void TrainerExtensionInstanceTests()
        {
            var context = new MLContext();
            var trainerNames = Enum.GetValues(typeof(TrainerName)).Cast<TrainerName>();
            foreach(var trainerName in trainerNames)
            {
                var extension = TrainerExtensionCatalog.GetTrainerExtension(trainerName);
                var instance = extension.CreateInstance(context, null);
                Assert.IsNotNull(instance);
                var sweepParams = extension.GetHyperparamSweepRanges();
                Assert.IsNotNull(sweepParams);
            }
        }

        [TestMethod]
        public void GetTrainersByMaxIterations()
        {
            var tasks = new TaskKind[] { TaskKind.BinaryClassification,
                TaskKind.MulticlassClassification, TaskKind.Regression  };

            foreach(var task in tasks)
            {
                var trainerSet10 = TrainerExtensionCatalog.GetTrainers(task, 10);
                var trainerSet50 = TrainerExtensionCatalog.GetTrainers(task, 50);
                var trainerSet100 = TrainerExtensionCatalog.GetTrainers(task, 100);

                Assert.IsNotNull(trainerSet10);
                Assert.IsNotNull(trainerSet50);
                Assert.IsNotNull(trainerSet100);

                Assert.IsTrue(trainerSet10.Count() < trainerSet50.Count());
                Assert.IsTrue(trainerSet50.Count() < trainerSet100.Count());
            }
        }

        [TestMethod]
        public void BuildPipelineNodePropsLightGbm()
        {
            var sweepParams = SweepableParams.BuildLightGbmParams();
            foreach(var sweepParam in sweepParams)
            {
                sweepParam.RawValue = 1;
            }

            var lightGbmBinaryProps = TrainerExtensionUtil.BuildPipelineNodeProps(TrainerName.LightGbmBinary, sweepParams);
            var lightGbmMultiProps = TrainerExtensionUtil.BuildPipelineNodeProps(TrainerName.LightGbmMulti, sweepParams);
            var lightGbmRegressionProps = TrainerExtensionUtil.BuildPipelineNodeProps(TrainerName.LightGbmRegression, sweepParams);

            var expectedJson = @"
{
  ""NumBoostRound"": 1,
  ""LearningRate"": 1,
  ""NumLeaves"": 1,
  ""MinDataPerLeaf"": 1,
  ""UseSoftmax"": 1,
  ""UseCat"": 1,
  ""UseMissing"": 1,
  ""MinDataPerGroup"": 1,
  ""MaxCatThreshold"": 1,
  ""CatSmooth"": 1,
  ""CatL2"": 1,
  ""TreeBooster"": {
    ""Name"": ""Microsoft.ML.LightGBM.TreeBooster"",
    ""Properties"": {
      ""RegLambda"": 1,
      ""RegAlpha"": 1
    }
  }
}";
            Util.AssertObjectMatchesJson(expectedJson, lightGbmBinaryProps);
            Util.AssertObjectMatchesJson(expectedJson, lightGbmMultiProps);
            Util.AssertObjectMatchesJson(expectedJson, lightGbmRegressionProps);
        }

        [TestMethod]
        public void BuildPipelineNodePropsSdca()
        {
            var sweepParams = SweepableParams.BuildSdcaParams();
            foreach(var sweepParam in sweepParams)
            {
                sweepParam.RawValue = 1;
            }

            var sdcaBinaryProps = TrainerExtensionUtil.BuildPipelineNodeProps(TrainerName.SdcaBinary, sweepParams);
            var expectedJson = @"
{
  ""L2Const"": 1,
  ""L1Threshold"": 1,
  ""ConvergenceTolerance"": 1,
  ""MaxIterations"": 1,
  ""Shuffle"": 1,
  ""BiasLearningRate"": 1
}";
            Util.AssertObjectMatchesJson(expectedJson, sdcaBinaryProps);
        }
        
        [TestMethod]
        public void BuildParameterSetLightGbm()
        {
            var props = new Dictionary<string, object>()
            {
                {"NumBoostRound", 1 },
                {"LearningRate", 1 },
                {"TreeBooster", new CustomProperty() {
                    Name = "Microsoft.ML.LightGBM.TreeBooster",
                    Properties = new Dictionary<string, object>()
                    {
                        {"RegLambda", 1 },
                        {"RegAlpha", 1 },
                    }
                } },
            };
            var binaryParams = TrainerExtensionUtil.BuildParameterSet(TrainerName.LightGbmBinary, props);
            var multiParams = TrainerExtensionUtil.BuildParameterSet(TrainerName.LightGbmMulti, props);
            var regressionParams = TrainerExtensionUtil.BuildParameterSet(TrainerName.LightGbmRegression, props);

            foreach(var paramSet in new ParameterSet[] { binaryParams, multiParams, regressionParams })
            {
                Assert.AreEqual(4, paramSet.Count);
                Assert.AreEqual("1", paramSet["NumBoostRound"].ValueText);
                Assert.AreEqual("1", paramSet["LearningRate"].ValueText);
                Assert.AreEqual("1", paramSet["RegLambda"].ValueText);
                Assert.AreEqual("1", paramSet["RegAlpha"].ValueText);
            }
        }

        [TestMethod]
        public void BuildParameterSetSdca()
        {
            var props = new Dictionary<string, object>()
            {
                {"LearningRate", 1 },
            };

            var sdcaParams = TrainerExtensionUtil.BuildParameterSet(TrainerName.SdcaBinary, props);
            
            Assert.AreEqual(1, sdcaParams.Count);
            Assert.AreEqual("1", sdcaParams["LearningRate"].ValueText);
        }
    }
}
