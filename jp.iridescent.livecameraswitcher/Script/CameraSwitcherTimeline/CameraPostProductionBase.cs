using System;
using UnityEngine;

namespace CameraLiveSwitcher
{
    [Serializable]
    public abstract class CameraPostProductionBase:ICameraPostProduction
    {
        
        public abstract void UpdateEffect(Camera camera);
        public abstract void Initialize();
        
        
    }
}