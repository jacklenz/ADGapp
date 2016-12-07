using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CraneGameeDue
{
    //This class is used to define in-game useful constants

    public class Cst
    {
        public const int screenWidth = 800;
        public const int screenHeight = 600;

        public const float rotAngleSpeedSmoother = .5f;
        public const float pi = 3.1415f;
        public const float mouseCamRotationDegree = .05f;

        public const float wheelScale = .4f;

        public const float heightOffset = .128f;

        //camera limits

        public const float minCamAngle = -2f;
        public const float maxCamAngle = 0f;

        //angles limits

        public const float thetaMin = 0f;  // This means we do NOT USE theta, because it would give us major problems with bounding boxes
        public const float thetaMax = 0f;
        public const float phiMin = -1f;
        public const float phiMax = .3f;
        public const float gammaMin = -1f;
        public const float gammaMax = 1f;

        //boundingBox

        public Vector3 boundingBoxMin = new Vector3();
        public Vector3 boundingBoxMax = new Vector3();
    }
}
