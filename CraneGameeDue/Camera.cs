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
    //This class defines a general camera
    public abstract class Camera
    {
        Matrix view;
        Matrix projection;
        public MouseState pms, cms;
        public Matrix Projection
        {
            get { return projection; }
            protected set
            {
                projection = value;
                generateFrustum();
            }
        }

        public Matrix View
        {
            get { return view; }
            protected set
            {
                view = value;
                generateFrustum();
            }
        }
        protected GraphicsDevice GraphicsDevice { get; set; }
        public BoundingFrustum Frustum { get; private set; }

        public Camera(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;
            generatePerspectiveProjectionMatrix(MathHelper.PiOver4);
        }
        private void generatePerspectiveProjectionMatrix(float FieldOfView)
        {
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            float aspectRatio = (float)pp.BackBufferWidth /
            (float)pp.BackBufferHeight;
            this.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), aspectRatio, 0.1f, 1000000.0f);
        }

        private void generateFrustum()
        {
            Matrix viewProjection = View * Projection;
            Frustum = new BoundingFrustum(viewProjection);
        }

        public bool BoundingVolumeIsInView(BoundingSphere sphere)
        {
            return (Frustum.Contains(sphere) != ContainmentType.Disjoint);
        }

        public bool BoundingVolumeIsInView(BoundingBox box)
        {
            return (Frustum.Contains(box) != ContainmentType.Disjoint);
        }

        public virtual void Update()
        {
        }
    }

    //Derived camera class, not used in-game
    public class TargetCamera : Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public TargetCamera(Vector3 Position, Vector3 Target,
        GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            this.Position = Position;
            this.Target = Target;
        }
        public override void Update()
        {
            Vector3 forward = Target - Position;
            Vector3 side = Vector3.Cross(forward, Vector3.Up);
            Vector3 up = Vector3.Cross(forward, side);
            this.View = Matrix.CreateLookAt(Position, Target, up);
        }
    }

    //Derived camera class, not used in-game
    public class FreeCamera : Camera
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Target { get; private set; }
        private Vector3 translation;
        public FreeCamera(Vector3 Position, float Yaw, float Pitch,
        GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            this.Position = Position;
            this.Yaw = Yaw;
            this.Pitch = Pitch;
            translation = Vector3.Zero;
        }

        public void Rotate(float YawChange, float PitchChange)
        {
            this.Yaw += YawChange;
            this.Pitch += PitchChange;
        }

        public void Move(Vector3 Translation)
        {
            this.translation += Translation;
        }

        public override void Update()
        {
            //Create the rotation matrix
            Matrix rotation = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0);

            //Offset the position and reset the translation
            translation = Vector3.Transform(translation, rotation);
            Position += translation;
            translation = Vector3.Zero;

            //Calculate the new target
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            Target = Position + forward;

            //Calculate the up vector
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);

            //Calculate the view matrix
            View = Matrix.CreateLookAt(Position,Target,up);
        }
    }

    //Derived camera class, not used in-game
    public class ArcBallCamera : Camera
    {
        // Rotation around the two axes
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        // Y axis rotation limits (radians)
        public float MinRotationY { get; set; }
        public float MaxRotationY { get; set; }
        // Distance between the target and camera
        public float Distance { get; set; }
        // Distance limits
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        // Calculated position and specified target
        public Vector3 Position { get; private set; }
        public Vector3 Target { get; set; }
        public ArcBallCamera(Vector3 Target, float RotationX,
        float RotationY, float MinRotationY, float MaxRotationY,
        float Distance, float MinDistance, float MaxDistance,
        GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            this.Target = Target;
            this.MinRotationY = MinRotationY;
            this.MaxRotationY = MaxRotationY;
            // Lock the y axis rotation between the min and max values
            this.RotationY = MathHelper.Clamp(RotationY, MinRotationY,
            MaxRotationY);
            this.RotationX = RotationX;
            this.MinDistance = MinDistance;
            this.MaxDistance = MaxDistance;
            // Lock the distance between the min and max values
            this.Distance = MathHelper.Clamp(Distance, MinDistance, MaxDistance);
        }
        public void Move(float DistanceChange)
        {
            this.Distance += DistanceChange;
            this.Distance = MathHelper.Clamp(Distance, MinDistance,
            MaxDistance);
        }
        public void Rotate(float RotationXChange, float RotationYChange)
        {
            this.RotationX += RotationXChange;
            this.RotationY += -RotationYChange;
            this.RotationY = MathHelper.Clamp(RotationY, MinRotationY,
            MaxRotationY);
        }
        public void Translate(Vector3 PositionChange)
        {
            this.Position += PositionChange;
        }
        public override void Update()
        {
            // Calculate rotation matrix from rotation values
            Matrix rotation = Matrix.CreateFromYawPitchRoll(RotationX, -
            RotationY, 0);
            // Translate down the Z axis by the desired distance
            // between the camera and object, then rotate that
            // vector to find the camera offset from the target
            Vector3 translation = new Vector3(0, 0, Distance);
            translation = Vector3.Transform(translation, rotation);
            Position = Target + translation;
            // Calculate the up vector from the rotation matrix
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            View = Matrix.CreateLookAt(Position, Target, up);
        }
    }

    //The derived camera class which is used in-game
    public class ChaseCamera : Camera
    {
        public Vector3 Position { get; private set; }
        public Vector3 Target { get; private set; }
        public Vector3 FollowTargetPosition { get; private set; }
        public Vector3 FollowTargetRotation { get; private set; }
        public Vector3 PositionOffset { get; set; }
        public Vector3 TargetOffset { get; set; }
        public Vector3 RelativeCameraRotation;
        float springiness = .15f;
        public float Springiness
        {
            get { return springiness; }
            set { springiness = MathHelper.Clamp(value, 0, 1); }
        }
        public ChaseCamera(Vector3 PositionOffset, Vector3 TargetOffset, Vector3 RelativeCameraRotation, GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            this.PositionOffset = PositionOffset;
            this.TargetOffset = TargetOffset;
            this.RelativeCameraRotation = RelativeCameraRotation;
        }
        public void Move(Vector3 NewFollowTargetPosition,
        Vector3 NewFollowTargetRotation)
        {
            this.FollowTargetPosition = NewFollowTargetPosition;
            this.FollowTargetRotation = NewFollowTargetRotation;
        }
        public void Rotate(Vector3 RotationChange)
        {
            this.RelativeCameraRotation += RotationChange;
        }
        public override void Update()
        {
            pms = cms;
            cms = Mouse.GetState();

            //Camera rotation moving the mouse (with a vertical limitation)
            if (cms.X > pms.X) RelativeCameraRotation.Y -= Cst.mouseCamRotationDegree;
            if (cms.X < pms.X) RelativeCameraRotation.Y += Cst.mouseCamRotationDegree;
            if (cms.Y > pms.Y && RelativeCameraRotation.X > Cst.minCamAngle) RelativeCameraRotation.X -= Cst.mouseCamRotationDegree;
            if (cms.Y < pms.Y && RelativeCameraRotation.X < Cst.maxCamAngle) RelativeCameraRotation.X += Cst.mouseCamRotationDegree;
            // Sum the rotations of the model and the camera to ensure it
            // is rotated to the correct position relative to the model's
            // rotation
            Vector3 combinedRotation = FollowTargetRotation + RelativeCameraRotation;
            // Calculate the rotation matrix for the camera
            Matrix rotation = Matrix.CreateFromYawPitchRoll(combinedRotation.Y, combinedRotation.X, combinedRotation.Z);
            // Calculate the position the camera would be without the spring
            // value, using the rotation matrix and target position
            Vector3 desiredPosition = FollowTargetPosition + Vector3.Transform(PositionOffset, rotation);
            // Interpolate between the current position and desired position
            Position = Vector3.Lerp(Position, desiredPosition, Springiness);
            // Calculate the new target using the rotation matrix
            Target = FollowTargetPosition + Vector3.Transform(TargetOffset, rotation);
            // Obtain the up vector from the matrix
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            // Recalculate the view matrix
            View = Matrix.CreateLookAt(Position, Target, up);
        }
    }
}
