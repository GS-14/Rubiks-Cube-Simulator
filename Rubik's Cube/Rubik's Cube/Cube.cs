using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Rubik_s_Cube
{
    class Cube
    {
        public Model Box;
        public Vector3 CameraPosition, InitialPosition, CurrentPosition;
        public int Xindex, Yindex, Zindex;
        public float Ratio;
        public Matrix WorldRotation, Rotation;


        public Cube(Model model, Vector3 pos, int x, int y, int z, float rat, Vector3 cameraPos, Matrix gameWorldRotation)
        {
            Box = model;
            InitialPosition = pos;
            CurrentPosition = InitialPosition;
            Xindex = x;
            Yindex = y;
            Zindex = z;
            Ratio = rat;
            CameraPosition = cameraPos;
            WorldRotation = gameWorldRotation;
            Rotation = WorldRotation;
        }


        public void Update(String key)
        {
            if (key == "FI")
            {
                if (CurrentPosition.Z >= 1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(6)));
                    Rotation *= Matrix.CreateRotationZ(MathHelper.ToRadians(6));
                }
            }

            else if (key == "F")
            {
                if (CurrentPosition.Z >= 1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(-6)));
                    Rotation *= Matrix.CreateRotationZ(MathHelper.ToRadians(-6));
                }
            }

            else if (key == "UI")
            {
                if (CurrentPosition.Y >= 1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(6)));
                    Rotation *= Matrix.CreateRotationY(MathHelper.ToRadians(6));
                }
            }

            else if (key == "U")
            {
                if (CurrentPosition.Y >= 1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(-6)));
                    Rotation *= Matrix.CreateRotationY(MathHelper.ToRadians(-6));
                }
            }

            else if (key == "RI")
            {
                if (CurrentPosition.X >= 1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(6)));
                    Rotation *= Matrix.CreateRotationX(MathHelper.ToRadians(6));
                }
            }

            else if (key == "R")
            {
                if (CurrentPosition.X >= 1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(-6)));
                    Rotation *= Matrix.CreateRotationX(MathHelper.ToRadians(-6));
                }
            }

            else if (key == "DI")
            {
                if (CurrentPosition.Y <= -1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(-6)));
                    Rotation *= Matrix.CreateRotationY(MathHelper.ToRadians(-6));
                }
            }

            else if (key == "D")
            {
                if (CurrentPosition.Y <= -1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(6)));
                    Rotation *= Matrix.CreateRotationY(MathHelper.ToRadians(6));
                }
            }

            else if (key == "LI")
            {
                if (CurrentPosition.X <= -1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(-6)));
                    Rotation *= Matrix.CreateRotationX(MathHelper.ToRadians(-6));
                }
            }

            else if (key == "L")
            {
                if (CurrentPosition.X <= -1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(6)));
                    Rotation *= Matrix.CreateRotationX(MathHelper.ToRadians(6));
                }
            }

            else if (key == "BI")
            {
                if (CurrentPosition.Z <= -1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(-6)));
                    Rotation *= Matrix.CreateRotationZ(MathHelper.ToRadians(-6));
                }
            }

            else if (key == "B")
            {
                if (CurrentPosition.Z <= -1)
                {
                    CurrentPosition = Vector3.Transform(CurrentPosition, Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(6)));
                    Rotation *= Matrix.CreateRotationZ(MathHelper.ToRadians(6));
                }
            }
        }


        public void Draw()
        {
            Matrix[] transforms = new Matrix[Box.Bones.Count];
            Box.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in Box.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    effect.DirectionalLight0.DiffuseColor = new Vector3(1, 1, 1);
                    effect.DirectionalLight0.Direction = new Vector3(0, 0, -1);
                    effect.DirectionalLight1.DiffuseColor = new Vector3(1, 1, 1);
                    effect.DirectionalLight1.Direction = new Vector3(0, -1, 0);
                    effect.DirectionalLight2.DiffuseColor = new Vector3(1, 1, 1);
                    effect.DirectionalLight2.Direction = new Vector3(0, 0, -1);
                    effect.World = transforms[mesh.ParentBone.Index] * Rotation * WorldRotation * Matrix.CreateTranslation(Vector3.Transform(CurrentPosition, WorldRotation));
                    effect.View = Matrix.CreateLookAt(CameraPosition, Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), Ratio, 1.0f, 1000.0f);
                }
                mesh.Draw();
            }
        }



    }
}
