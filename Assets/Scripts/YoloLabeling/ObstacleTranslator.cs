
using System;
using System.Collections.Generic;
using YoloHolo.Services;

namespace YoloHolo.YoloLabeling
{
    [Serializable]
    public class ObstacleTranslator : IYoloClassTranslator
    {
        public string GetName(int classIndex)
        {
            return detectableObjects[classIndex];
        }

        private static List<string> detectableObjects = new()
        {
            "obstacle",
            "target",
            "wall",
        };
    }
}
