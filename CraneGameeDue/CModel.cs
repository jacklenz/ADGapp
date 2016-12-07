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
    //The model class defines the structure for each model in the scene
    public class CModel
    {
        //Model Parameters
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public Model Model;
        public Matrix[] modelTransforms;
        private GraphicsDevice graphicsDevice;
        public BoundingBox boundingBox;
        public Vector3 boundingBoxMin, boundingBoxMax;

        public CModel(Model Model, Vector3 Position, Vector3 Rotation, Vector3 Scale, GraphicsDevice graphicsDevice)
        {
            this.Model = Model;
            modelTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
            this.graphicsDevice = graphicsDevice;
            transformBoundingBox();
        }

        //Here we only call the updating of the model's boundingbox
        public virtual void Update(GameTime gameTime)
        {
            transformBoundingBox();
        }

        //The method which draws the boundingbox's edges
        private void DrawBoundingBox(BoundingBoxBuffers buffers, BasicEffect effect, GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
            graphicsDevice.SetVertexBuffer(buffers.Vertices);
            graphicsDevice.Indices = buffers.Indices;

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, buffers.VertexCount, 0, buffers.PrimitiveCount);
            }
        }

        public void Draw(Matrix View, Matrix Projection)
        {
            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            Matrix baseWorld = Matrix.CreateScale(Scale)*Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)*Matrix.CreateTranslation(Position);
            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]*baseWorld;
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BasicEffect effect = (BasicEffect)meshPart.Effect;
                    effect.World = localWorld;
                    effect.View = View;
                    effect.Projection = Projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

            //Create a BoundingBoxBuffers instance and a basic effect to draw our boundingbox
            BoundingBoxBuffers bbBuff = CreateBoundingBoxBuffers(boundingBox,graphicsDevice);
            BasicEffect lineEffect = new BasicEffect(graphicsDevice);
            lineEffect.LightingEnabled = false;
            lineEffect.TextureEnabled = false;
            lineEffect.VertexColorEnabled = true;
            //Draw the boundingbox
            DrawBoundingBox(bbBuff,lineEffect,graphicsDevice,View,Projection);
        }

        //This method builds the drawable object related to the boundingbox limits
        private BoundingBoxBuffers CreateBoundingBoxBuffers(BoundingBox bBox, GraphicsDevice graphicsDevice)
        {
            BoundingBoxBuffers boundingBoxBuffers = new BoundingBoxBuffers();

            boundingBoxBuffers.PrimitiveCount = 24;
            boundingBoxBuffers.VertexCount = 48;

            VertexBuffer vertexBuffer = new VertexBuffer(graphicsDevice,
                typeof(VertexPositionColor), boundingBoxBuffers.VertexCount,
                BufferUsage.WriteOnly);
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            const float ratio = 5.0f;

            Vector3 xOffset = new Vector3((bBox.Max.X - bBox.Min.X) / ratio, 0, 0);
            Vector3 yOffset = new Vector3(0, (bBox.Max.Y - bBox.Min.Y) / ratio, 0);
            Vector3 zOffset = new Vector3(0, 0, (bBox.Max.Z - bBox.Min.Z) / ratio);
            Vector3[] corners = bBox.GetCorners();

            // Corner 1.
            AddVertex(vertices, corners[0]);
            AddVertex(vertices, corners[0] + xOffset);
            AddVertex(vertices, corners[0]);
            AddVertex(vertices, corners[0] - yOffset);
            AddVertex(vertices, corners[0]);
            AddVertex(vertices, corners[0] - zOffset);

            // Corner 2.
            AddVertex(vertices, corners[1]);
            AddVertex(vertices, corners[1] - xOffset);
            AddVertex(vertices, corners[1]);
            AddVertex(vertices, corners[1] - yOffset);
            AddVertex(vertices, corners[1]);
            AddVertex(vertices, corners[1] - zOffset);

            // Corner 3.
            AddVertex(vertices, corners[2]);
            AddVertex(vertices, corners[2] - xOffset);
            AddVertex(vertices, corners[2]);
            AddVertex(vertices, corners[2] + yOffset);
            AddVertex(vertices, corners[2]);
            AddVertex(vertices, corners[2] - zOffset);

            // Corner 4.
            AddVertex(vertices, corners[3]);
            AddVertex(vertices, corners[3] + xOffset);
            AddVertex(vertices, corners[3]);
            AddVertex(vertices, corners[3] + yOffset);
            AddVertex(vertices, corners[3]);
            AddVertex(vertices, corners[3] - zOffset);

            // Corner 5.
            AddVertex(vertices, corners[4]);
            AddVertex(vertices, corners[4] + xOffset);
            AddVertex(vertices, corners[4]);
            AddVertex(vertices, corners[4] - yOffset);
            AddVertex(vertices, corners[4]);
            AddVertex(vertices, corners[4] + zOffset);

            // Corner 6.
            AddVertex(vertices, corners[5]);
            AddVertex(vertices, corners[5] - xOffset);
            AddVertex(vertices, corners[5]);
            AddVertex(vertices, corners[5] - yOffset);
            AddVertex(vertices, corners[5]);
            AddVertex(vertices, corners[5] + zOffset);

            // Corner 7.
            AddVertex(vertices, corners[6]);
            AddVertex(vertices, corners[6] - xOffset);
            AddVertex(vertices, corners[6]);
            AddVertex(vertices, corners[6] + yOffset);
            AddVertex(vertices, corners[6]);
            AddVertex(vertices, corners[6] + zOffset);

            // Corner 8.
            AddVertex(vertices, corners[7]);
            AddVertex(vertices, corners[7] + xOffset);
            AddVertex(vertices, corners[7]);
            AddVertex(vertices, corners[7] + yOffset);
            AddVertex(vertices, corners[7]);
            AddVertex(vertices, corners[7] + zOffset);

            vertexBuffer.SetData(vertices.ToArray());
            boundingBoxBuffers.Vertices = vertexBuffer;

            IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, boundingBoxBuffers.VertexCount, BufferUsage.WriteOnly);
            indexBuffer.SetData(Enumerable.Range(0, boundingBoxBuffers.VertexCount).Select(i => (short)i).ToArray());
            boundingBoxBuffers.Indices = indexBuffer;

            return boundingBoxBuffers;
        }

        //Method which adds a specified vertex to a vertexes list
        private static void AddVertex(List<VertexPositionColor> vertices, Vector3 position)
        {
            vertices.Add(new VertexPositionColor(position, Color.White));
        }

        //Create a bounding box for a ModelMeshPart
        private static BoundingBox? GetBoundingBox(ModelMeshPart meshPart, Matrix transform)
        {
            if (meshPart.VertexBuffer == null)
                return null;

            //Extract the vertices from the meshPart
            Vector3[] positions = VertexElementExtractor.GetVertexElement(meshPart, VertexElementUsage.Position);
            if (positions == null)
                return null;

            Vector3[] transformedPositions = new Vector3[positions.Length];
            Vector3.Transform(positions, ref transform, transformedPositions);

            return BoundingBox.CreateFromPoints(transformedPositions);
        }

        //Method to loop through each ModelMesh and ModelMeshPart within the Model, and create the merged boundingbox for the whole model
        public void buildBoundingBox()
        {
            boundingBox = new BoundingBox();
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BoundingBox? meshPartBoundingBox = GetBoundingBox(meshPart, modelTransforms[mesh.ParentBone.Index]);
                    if (meshPartBoundingBox != null)
                        boundingBox = BoundingBox.CreateMerged(boundingBox, meshPartBoundingBox.Value);
                }
            boundingBoxMin = boundingBox.Min;
            boundingBoxMax = boundingBox.Max;
        }

        //Method to update the boundingbox
        public virtual void transformBoundingBox()
        {
            //build the boundingbox
            buildBoundingBox();
            //Update its position
            boundingBox.Min = Position + boundingBoxMin;
            boundingBox.Max = Position + boundingBoxMax;
        }
    }



    /*  
        PLAYER CLASS
        The Player class is a derivate of the CModel class, implementing all the other stuff specifically designed for the player.

        PLAYER MESHES
        2   front wheels
        9
        8   back wheels
        10
        11  basic sphere
        12  first arm

        16  second arm
        
        17  hook 
    */
    public class Player : CModel
    {
        //Define the player's additive parameters
        public KeyboardState pks, cks;
        public float speed, wheelRot, steerRot;
        public float theta, phi, gamma;
        public bool trysCatching;
        public bool hasLost, hasWon;
        float trasX1, trasY1, trasZ1;
        float trasX2, trasY2, trasZ2;
        float trasX3, trasY3, trasZ3;

        public Player(Model Model, Vector3 Position, Vector3 Rotation, Vector3 Scale, GraphicsDevice graphicsDevice, float speed = 5f) : base(Model, Position, Rotation, Scale, graphicsDevice)
        {
            this.speed = speed;
            wheelRot = 0f;
            steerRot = 0f;
            theta = phi = gamma = 0f;
            trasX1 = trasY1 = trasZ1 = 0f;
            trasX2 = trasY2 = trasZ2 = 0f;
            trasX3 = trasY3 = trasZ3 = 0f;
            trysCatching = hasLost = hasWon = false;
        }

        //Override the bounding box transform in order to also take care of the rotation of the player's model
        public override void transformBoundingBox()
        {
            //build the boundingbox
            buildBoundingBox();
            //Define the translational components of the boundingbox's max and min constraints
            Vector2 XZMin = new Vector2(boundingBoxMin.X, boundingBoxMin.Z);
            Vector3 bbTrasMin = new Vector3(-(XZMin.Length() * (float)Math.Sin(Rotation.Y)), boundingBoxMin.Y, -(XZMin.Length() * (float)Math.Cos(Rotation.Y)));
            Vector2 XZMax = new Vector2(boundingBoxMax.X, boundingBoxMax.Z);
            Vector3 bbTrasMax = new Vector3((XZMax.Length() * (float)Math.Sin(Rotation.Y)), boundingBoxMax.Y, (XZMax.Length() * (float)Math.Cos(Rotation.Y)));
            //Little swap trick to prevent the min to become greater than the max and viceversa
            if (bbTrasMin.X > bbTrasMax.X)
            {
                float tmp = bbTrasMax.X;
                bbTrasMax.X = bbTrasMin.X;
                bbTrasMin.X = tmp;
            }
            if (bbTrasMin.Z > bbTrasMax.Z)
            {
                float tmp = bbTrasMax.Z;
                bbTrasMax.Z = bbTrasMin.Z;
                bbTrasMin.Z = tmp;
            }
            //Update the boundingbox's position basing on the player's one
            boundingBox.Min = Position + bbTrasMin;
            boundingBox.Max = Position + bbTrasMax;
        }

        public override void Update(GameTime gameTime)
        {
            pks = cks;
            cks = Keyboard.GetState();

            //Winning and losing constraints
            if (hasWon) hasLost = false;
            if (hasLost) hasWon = false;

            //Call the function to update the boundingbox
            transformBoundingBox();

            #region WheelsMovement
            
            //Split the speed in it's X and Z component
            float speedx = (float)((Math.Sin(Rotation.Y)) * speed);
            float speedz = (float)((Math.Cos(Rotation.Y)) * speed);
                        
            //Traslazione e rotazione delle ruote
            modelTransforms[2] = Matrix.CreateScale(Cst.wheelScale) * Matrix.CreateRotationX(wheelRot) * Matrix.CreateRotationY(steerRot) * Matrix.CreateTranslation(modelTransforms[2].Translation);
            modelTransforms[9] = Matrix.CreateScale(Cst.wheelScale) * Matrix.CreateRotationX(wheelRot) * Matrix.CreateRotationY(steerRot) * Matrix.CreateTranslation(modelTransforms[9].Translation);
            modelTransforms[10] = Matrix.CreateScale(Cst.wheelScale) * Matrix.CreateRotationX(wheelRot) * Matrix.CreateTranslation(modelTransforms[10].Translation);
            modelTransforms[8] = Matrix.CreateScale(Cst.wheelScale) * Matrix.CreateRotationX(wheelRot) * Matrix.CreateTranslation(modelTransforms[8].Translation);
            

            if (cks.IsKeyDown(Keys.Up) && !hasLost && !hasWon)
            {
                wheelRot += .2f;
                Position.X += speedx;
                Position.Z += speedz;

                if (cks.IsKeyDown(Keys.Left))
                {
                    Rotation.Y += speed * Cst.rotAngleSpeedSmoother;
                    if (steerRot < .5f) steerRot += .1f;
                }
                else if (cks.IsKeyDown(Keys.Right))
                {
                    Rotation.Y -= speed * Cst.rotAngleSpeedSmoother;
                    if (steerRot > -.5f) steerRot -= .1f;
                }
                else if(steerRot>0)
                    steerRot -= .1f;
                else if (steerRot < 0)
                    steerRot += .1f;
            }
            if (cks.IsKeyDown(Keys.Down) && !hasWon && !hasWon)
            {
                wheelRot -= .2f;
                Position.X -= speedx;
                Position.Z -= speedz;

                if (cks.IsKeyDown(Keys.Left))
                {
                    Rotation.Y -= speed * Cst.rotAngleSpeedSmoother;
                    if (steerRot < .5f) steerRot += .1f;
                }
                else if (cks.IsKeyDown(Keys.Right))
                {
                    Rotation.Y += speed * Cst.rotAngleSpeedSmoother;
                    if (steerRot > -.5f) steerRot -= .1f;
                }
                else if (steerRot > 0)
                    steerRot -= .1f;
                else if (steerRot < 0)
                    steerRot += .1f;
            }
            
            #endregion

            #region CraneMovement
            
            #region Arm1

            //First arm vertical rotation
            if (cks.IsKeyDown(Keys.W) && phi < Cst.phiMax)
            {
                phi += .1f;               
            }
            if (cks.IsKeyDown(Keys.S) && phi > Cst.phiMin)
            {
                phi -= .1f;
            }
            //First arm horizontal rotation (not used in game)
            if (cks.IsKeyDown(Keys.A) && theta < Cst.thetaMax)
            {
                theta += .1f;
            }
            if (cks.IsKeyDown(Keys.D) && theta > Cst.thetaMin)
            {
                theta -= .1f;
            }

            //Apply rotation and translation transforms to the first arm
            modelTransforms[12] = Matrix.CreateScale(1f) * Matrix.CreateRotationX(phi) * Matrix.CreateRotationY(theta) * Matrix.CreateTranslation(modelTransforms[12].Translation);
            #endregion

            #region Arm2

            //Define second arm translation components
            trasX1 = Cst.heightOffset + (float)Math.Sin(theta) * (float)Math.Cos(phi);
            trasY1 = -(float)Math.Sin(phi);
            trasZ1 = (float)Math.Cos(phi) * (float)Math.Cos(theta);

            //Apply the transforms to each mesh linked to the second arm ..
            for(int i = 14; i<16; i++)
                modelTransforms[i].Translation = modelTransforms[12].Translation + new Vector3(trasX1, trasY1, trasZ1);
            modelTransforms[16].Translation = modelTransforms[12].Translation + new Vector3(trasX1 - Cst.heightOffset, trasY1, trasZ1);
            // .. and to the second arm
            for (int i = 14; i < 17; i++)
                modelTransforms[i] = Matrix.CreateScale(1f) * Matrix.CreateRotationX(phi+gamma) * Matrix.CreateRotationY(theta) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(modelTransforms[16].Translation);

            //second arm vertical rotation
            if (cks.IsKeyDown(Keys.T) && gamma < Cst.gammaMax)
            {
                gamma += .1f;
            }
            if (cks.IsKeyDown(Keys.G) && gamma > Cst.gammaMin)
            {
                gamma -= .1f;
            }
            #endregion

            #region Hook

            //Define hook translation components
            trasX2 = ((float)Math.Sin(theta)) * ((float)Math.Cos(phi) + (float)Math.Cos(phi + gamma));
            trasY2 = - (float)Math.Sin(phi) - (float)Math.Sin(phi + gamma);
            trasZ2 = ((float)Math.Cos(theta)) * ((float)Math.Cos(phi) + (float)Math.Cos(phi + gamma));
            //Unify them
            for (int i = 17; i < 19; i++)
                modelTransforms[i].Translation = modelTransforms[12].Translation + new Vector3(trasX2, trasY2, trasZ2);
            //And apply the transforms
            for (int i = 17; i < 19; i++)
                modelTransforms[i] = Matrix.CreateScale(1f) * Matrix.CreateRotationX(phi + gamma) * Matrix.CreateRotationY(theta) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(modelTransforms[i].Translation);

            //Define the "catch" mode translation components of the hook
            trasX3 = .1f * ((float)Math.Sin(theta)) * ((float)Math.Cos(phi + gamma));
            trasY3 = .1f * (-(float)Math.Sin(phi + gamma));
            trasZ3 = .1f * ((float)Math.Cos(theta)) * (float)Math.Cos(phi + gamma);

            //Toggle the "catch" mode ON or OFF using the "U" key
            if (pks.IsKeyUp(Keys.U) && cks.IsKeyDown(Keys.U))
                trysCatching = !trysCatching;
            if (trysCatching)
                modelTransforms[17] = Matrix.CreateScale(1f) * Matrix.CreateRotationX(phi + gamma) * Matrix.CreateRotationY(theta) * Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(modelTransforms[17].Translation + new Vector3(trasX3, trasY3, trasZ3));

            #endregion

            #endregion
        }
    }



    //This class extracts the vertex positions of the model
    public static class VertexElementExtractor
    {
        public static Vector3[] GetVertexElement(ModelMeshPart meshPart, VertexElementUsage usage)
        {
            VertexDeclaration vd = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] elements = vd.GetVertexElements();

            Func<VertexElement, bool> elementPredicate = ve => ve.VertexElementUsage == usage && ve.VertexElementFormat == VertexElementFormat.Vector3;
            if (!elements.Any(elementPredicate))
                return null;

            VertexElement element = elements.First(elementPredicate);

            Vector3[] vertexData = new Vector3[meshPart.NumVertices];
            meshPart.VertexBuffer.GetData((meshPart.VertexOffset * vd.VertexStride) + element.Offset, vertexData, 0, vertexData.Length, vd.VertexStride);

            return vertexData;
        }
    }

    //This class holds the vertex and index buffers to draw the bounding box edges
    public class BoundingBoxBuffers
    {
        public VertexBuffer Vertices;
        public int VertexCount;
        public IndexBuffer Indices;
        public int PrimitiveCount;
    }

}
