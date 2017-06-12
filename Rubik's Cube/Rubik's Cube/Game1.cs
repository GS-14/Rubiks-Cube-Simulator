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
using Rubik_s_Cube;

namespace Rubik_s_Cube
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model cube;
        float ratio;
        int rotationX, rotationY, rotationZ;
        Vector3 cameraPos;
        Matrix gameWorldRotation;
        Cube[, ,] rubiksCube;
        int[, ,] data;
        int time;
        int cycle;
        int moveNumber;
        KeyboardState state;
        String key;
        Boolean solveMode, crossSolved, centersAlligned, firstLayer, secondLayer, backCross, backFace, backCorners, backEdges;
        List<string> moves;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            rotationX = 0;
            rotationY = 0;
            rotationZ = 0;
            time = 0;
            cycle = -1;
            moveNumber = 0;
            ratio = graphics.GraphicsDevice.Viewport.AspectRatio;
            cameraPos = new Vector3(0, 0, 20);
            gameWorldRotation = Matrix.Identity;
            cube = Content.Load<Model>("Cube Model");
            state = Keyboard.GetState();

            rubiksCube = new Cube[3, 3, 3];

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        rubiksCube[x, y, z] = new Cube(cube, new Vector3(2 * (x - 1), 2 * (y - 1), 2 * (z - 1)), x, y, z, ratio, cameraPos, gameWorldRotation);
                    }
                }
            }

            data = new int[6, 3, 3];

            for (int a = 0; a < 6; a++)
            {
                for (int b = 0; b < 3; b++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        data[a, b, c] = a;
                    }
                }
            }

            solveMode = false;
            crossSolved = true;
            centersAlligned = true;
            firstLayer = true;
            secondLayer = true;
            backCross = true;
            backFace = true;
            backCorners = true;
            backEdges = true;
            moves = new List<string>();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here


            time++;

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                rotationY += 5;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                rotationY -= 5;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                rotationX -= 5;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                rotationX += 5;
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                rotationZ -= 5;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                rotationZ += 5;
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                rotationX = 0;
                rotationY = 0;
                rotationZ = 0;
            }
            gameWorldRotation = Matrix.CreateRotationX(MathHelper.ToRadians(rotationX % 360)) * Matrix.CreateRotationY(MathHelper.ToRadians(rotationY % 360)) * Matrix.CreateRotationZ(MathHelper.ToRadians(rotationZ % 360));

            foreach (Cube element in rubiksCube)
            {
                element.WorldRotation = gameWorldRotation;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && solveMode == false)
            {
                moveNumber = 0;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && solveMode == false)
            {
                moves = new List<string>();
                moveNumber = 0;
                crossSolved = false;
                centersAlligned = false;
                firstLayer = false;
                secondLayer = false;
                backCross = false;
                backFace = false;
                backCorners = false;
                backEdges = false;
                solveMode = true;
                solveCross();
                allignCenters();
                finishFirstLayer();
                finishSecondLayer();
                solveBackCross();
                finishBackFace();
                positionBackCorners();
                positionBackEdges();
                //removeTriples();
            }

            if (time % 5 == 0 && cycle == -1 && solveMode == true)
            {
                if (moveNumber < moves.Count)
                {
                    foreach (Cube cube in rubiksCube)
                    {
                        cube.Update(moves.ElementAt(moveNumber));
                    }
                    key = moves.ElementAt(moveNumber);
                    cycle++;
                }
                else
                {
                    solveMode = false;
                    moveNumber = 0;
                }
            }

            if (time % 5 == 0 && cycle == -1 && solveMode == false)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.D1) ||
                    Keyboard.GetState().IsKeyDown(Keys.D2) ||
                    Keyboard.GetState().IsKeyDown(Keys.D3) ||
                    Keyboard.GetState().IsKeyDown(Keys.D4) ||
                    Keyboard.GetState().IsKeyDown(Keys.D5) ||
                    Keyboard.GetState().IsKeyDown(Keys.D6))
                {
                    state = Keyboard.GetState();
                    if (state.IsKeyDown(Keys.D1) && state.IsKeyDown(Keys.LeftShift))
                    {
                        key = "FI";
                    }

                    else if (state.IsKeyDown(Keys.D1))
                    {
                        key = "F";
                    }

                    else if (state.IsKeyDown(Keys.D2) && state.IsKeyDown(Keys.LeftShift))
                    {
                        key = "UI";
                    }

                    else if (state.IsKeyDown(Keys.D2))
                    {
                        key = "U";
                    }

                    else if (state.IsKeyDown(Keys.D3) && state.IsKeyDown(Keys.LeftShift))
                    {
                        key = "RI";
                    }

                    else if (state.IsKeyDown(Keys.D3))
                    {
                        key = "R";
                    }

                    else if (state.IsKeyDown(Keys.D4) && state.IsKeyDown(Keys.LeftShift))
                    {
                        key = "DI";
                    }

                    else if (state.IsKeyDown(Keys.D4))
                    {
                        key = "D";
                    }

                    else if (state.IsKeyDown(Keys.D5) && state.IsKeyDown(Keys.LeftShift))
                    {
                        key = "LI";
                    }

                    else if (state.IsKeyDown(Keys.D5))
                    {
                        key = "L";
                    }

                    else if (state.IsKeyDown(Keys.D6) && state.IsKeyDown(Keys.LeftShift))
                    {
                        key = "BI";
                    }

                    else if (state.IsKeyDown(Keys.D6))
                    {
                        key = "B";
                    }

                    turn(key);
                    //printData();

                    foreach (Cube cube in rubiksCube)
                    {
                        cube.Update(key);
                    }

                    cycle++;
                }
            }

            if (cycle != -1)
            {
                foreach (Cube cube in rubiksCube)
                {
                    cube.Update(key);
                }
                cycle++;
            }

            if (cycle == 14)
            {
                cycle = -1;
                moveNumber++;
                Console.WriteLine("Move Number: " + moveNumber);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            

            foreach (Cube a in rubiksCube)
            {
                a.Draw();
            }

            base.Draw(gameTime);

            
        }

        public void printData()
        {
            for (int a = 0; a < 6; a++)
            {
                Console.WriteLine("Face: " + a);
                for (int b = 0; b < 3; b++)
                {
                    Console.WriteLine(data[a, b, 0] + " " + data[a, b, 1] + " " + data[a, b, 2]);
                }
            }
        }

        public void turn(string side)
        {
            int face, temp;

            if (side == "F")
            {
                face = 0;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 2, 0];
                data[face, 2, 0] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 0, 2];
                data[face, 0, 2] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 0];
                data[face, 1, 0] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 2];
                data[face, 1, 2] = temp;

                temp = data[1, 2, 0];
                data[1, 2, 0] = data[4, 2, 2];
                data[4, 2, 2] = data[3, 0, 2];
                data[3, 0, 2] = data[2, 0, 0];
                data[2, 0, 0] = temp;

                temp = data[1, 2, 1];
                data[1, 2, 1] = data[4, 1, 2];
                data[4, 1, 2] = data[3, 0, 1];
                data[3, 0, 1] = data[2, 1, 0];
                data[2, 1, 0] = temp;

                temp = data[1, 2, 2];
                data[1, 2, 2] = data[4, 0, 2];
                data[4, 0, 2] = data[3, 0, 0];
                data[3, 0, 0] = data[2, 2, 0];
                data[2, 2, 0] = temp;
            }
            else if (side == "FI")
            {
                face = 0;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 0, 2];
                data[face, 0, 2] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 2, 0];
                data[face, 2, 0] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 2];
                data[face, 1, 2] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 0];
                data[face, 1, 0] = temp;

                temp = data[1, 2, 0];
                data[1, 2, 0] = data[2, 0, 0];
                data[2, 0, 0] = data[3, 0, 2];
                data[3, 0, 2] = data[4, 2, 2];
                data[4, 2, 2] = temp;

                temp = data[1, 2, 1];
                data[1, 2, 1] = data[2, 1, 0];
                data[2, 1, 0] = data[3, 0, 1];
                data[3, 0, 1] = data[4, 1, 2];
                data[4, 1, 2] = temp;

                temp = data[1, 2, 2];
                data[1, 2, 2] = data[2, 2, 0];
                data[2, 2, 0] = data[3, 0, 0];
                data[3, 0, 0] = data[4, 0, 2];
                data[4, 0, 2] = temp;
            }
            else if (side == "U")
            {
                face = 1;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 2, 0];
                data[face, 2, 0] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 0, 2];
                data[face, 0, 2] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 0];
                data[face, 1, 0] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 2];
                data[face, 1, 2] = temp;

                temp = data[0, 0, 0];
                data[0, 0, 0] = data[2, 0, 0];
                data[2, 0, 0] = data[5, 0, 0];
                data[5, 0, 0] = data[4, 0, 0];
                data[4, 0, 0] = temp;

                temp = data[0, 0, 1];
                data[0, 0, 1] = data[2, 0, 1];
                data[2, 0, 1] = data[5, 0, 1];
                data[5, 0, 1] = data[4, 0, 1];
                data[4, 0, 1] = temp;

                temp = data[0, 0, 2];
                data[0, 0, 2] = data[2, 0, 2];
                data[2, 0, 2] = data[5, 0, 2];
                data[5, 0, 2] = data[4, 0, 2];
                data[4, 0, 2] = temp;
            }
            else if (side == "UI")
            {
                face = 1;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 0, 2];
                data[face, 0, 2] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 2, 0];
                data[face, 2, 0] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 2];
                data[face, 1, 2] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 0];
                data[face, 1, 0] = temp;

                temp = data[0, 0, 0];
                data[0, 0, 0] = data[4, 0, 0];
                data[4, 0, 0] = data[5, 0, 0];
                data[5, 0, 0] = data[2, 0, 0];
                data[2, 0, 0] = temp;

                temp = data[0, 0, 1];
                data[0, 0, 1] = data[4, 0, 1];
                data[4, 0, 1] = data[5, 0, 1];
                data[5, 0, 1] = data[2, 0, 1];
                data[2, 0, 1] = temp;

                temp = data[0, 0, 2];
                data[0, 0, 2] = data[4, 0, 2];
                data[4, 0, 2] = data[5, 0, 2];
                data[5, 0, 2] = data[2, 0, 2];
                data[2, 0, 2] = temp;
            }
            else if (side == "R")
            {
                face = 2;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 2, 0];
                data[face, 2, 0] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 0, 2];
                data[face, 0, 2] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 0];
                data[face, 1, 0] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 2];
                data[face, 1, 2] = temp; ;

                temp = data[0, 0, 2];
                data[0, 0, 2] = data[3, 0, 2];
                data[3, 0, 2] = data[5, 2, 0];
                data[5, 2, 0] = data[1, 0, 2];
                data[1, 0, 2] = temp;

                temp = data[0, 1, 2];
                data[0, 1, 2] = data[3, 1, 2];
                data[3, 1, 2] = data[5, 1, 0];
                data[5, 1, 0] = data[1, 1, 2];
                data[1, 1, 2] = temp;

                temp = data[0, 2, 2];
                data[0, 2, 2] = data[3, 2, 2];
                data[3, 2, 2] = data[5, 0, 0];
                data[5, 0, 0] = data[1, 2, 2];
                data[1, 2, 2] = temp;

            }
            else if (side == "RI")
            {
                face = 2;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 0, 2];
                data[face, 0, 2] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 2, 0];
                data[face, 2, 0] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 2];
                data[face, 1, 2] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 0];
                data[face, 1, 0] = temp;

                temp = data[0, 0, 2];
                data[0, 0, 2] = data[1, 0, 2];
                data[1, 0, 2] = data[5, 2, 0];
                data[5, 2, 0] = data[3, 0, 2];
                data[3, 0, 2] = temp;

                temp = data[0, 1, 2];
                data[0, 1, 2] = data[1, 1, 2];
                data[1, 1, 2] = data[5, 1, 0];
                data[5, 1, 0] = data[3, 1, 2];
                data[3, 1, 2] = temp;

                temp = data[0, 2, 2];
                data[0, 2, 2] = data[1, 2, 2];
                data[1, 2, 2] = data[5, 0, 0];
                data[5, 0, 0] = data[3, 2, 2];
                data[3, 2, 2] = temp;
            }
            else if (side == "D")
            {
                face = 3;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 2, 0];
                data[face, 2, 0] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 0, 2];
                data[face, 0, 2] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 0];
                data[face, 1, 0] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 2];
                data[face, 1, 2] = temp;

                temp = data[0, 2, 0];
                data[0, 2, 0] = data[4, 2, 0];
                data[4, 2, 0] = data[5, 2, 0];
                data[5, 2, 0] = data[2, 2, 0];
                data[2, 2, 0] = temp;

                temp = data[0, 2, 1];
                data[0, 2, 1] = data[4, 2, 1];
                data[4, 2, 1] = data[5, 2, 1];
                data[5, 2, 1] = data[2, 2, 1];
                data[2, 2, 1] = temp;

                temp = data[0, 2, 2];
                data[0, 2, 2] = data[4, 2, 2];
                data[4, 2, 2] = data[5, 2, 2];
                data[5, 2, 2] = data[2, 2, 2];
                data[2, 2, 2] = temp;
            }
            else if (side == "DI")
            {
                face = 3;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 0, 2];
                data[face, 0, 2] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 2, 0];
                data[face, 2, 0] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 2];
                data[face, 1, 2] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 0];
                data[face, 1, 0] = temp;

                temp = data[0, 2, 0];
                data[0, 2, 0] = data[2, 2, 0];
                data[2, 2, 0] = data[5, 2, 0];
                data[5, 2, 0] = data[4, 2, 0];
                data[4, 2, 0] = temp;

                temp = data[0, 2, 1];
                data[0, 2, 1] = data[2, 2, 1];
                data[2, 2, 1] = data[5, 2, 1];
                data[5, 2, 1] = data[4, 2, 1];
                data[4, 2, 1] = temp;

                temp = data[0, 2, 2];
                data[0, 2, 2] = data[2, 2, 2];
                data[2, 2, 2] = data[5, 2, 2];
                data[5, 2, 2] = data[4, 2, 2];
                data[4, 2, 2] = temp;
            }
            else if (side == "L")
            {
                face = 4;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 2, 0];
                data[face, 2, 0] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 0, 2];
                data[face, 0, 2] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 0];
                data[face, 1, 0] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 2];
                data[face, 1, 2] = temp;

                temp = data[0, 0, 0];
                data[0, 0, 0] = data[1, 0, 0];
                data[1, 0, 0] = data[5, 2, 2];
                data[5, 2, 2] = data[3, 0, 0];
                data[3, 0, 0] = temp;

                temp = data[0, 1, 0];
                data[0, 1, 0] = data[1, 1, 0];
                data[1, 1, 0] = data[5, 1, 2];
                data[5, 1, 2] = data[3, 1, 0];
                data[3, 1, 0] = temp;

                temp = data[0, 2, 0];
                data[0, 2, 0] = data[1, 2, 0];
                data[1, 2, 0] = data[5, 0, 2];
                data[5, 0, 2] = data[3, 2, 0];
                data[3, 2, 0] = temp;
            }
            else if (side == "LI")
            {
                face = 4;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 0, 2];
                data[face, 0, 2] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 2, 0];
                data[face, 2, 0] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 2];
                data[face, 1, 2] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 0];
                data[face, 1, 0] = temp;

                temp = data[0, 0, 0];
                data[0, 0, 0] = data[3, 0, 0];
                data[3, 0, 0] = data[5, 2, 2];
                data[5, 2, 2] = data[1, 0, 0];
                data[1, 0, 0] = temp;

                temp = data[0, 1, 0];
                data[0, 1, 0] = data[3, 1, 0];
                data[3, 1, 0] = data[5, 1, 2];
                data[5, 1, 2] = data[1, 1, 0];
                data[1, 1, 0] = temp;

                temp = data[0, 2, 0];
                data[0, 2, 0] = data[3, 2, 0];
                data[3, 2, 0] = data[5, 0, 2];
                data[5, 0, 2] = data[1, 2, 0];
                data[1, 2, 0] = temp;
            }
            else if (side == "B")
            {
                face = 5;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 2, 0];
                data[face, 2, 0] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 0, 2];
                data[face, 0, 2] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 0];
                data[face, 1, 0] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 2];
                data[face, 1, 2] = temp;

                temp = data[1, 0, 0];
                data[1, 0, 0] = data[2, 0, 2];
                data[2, 0, 2] = data[3, 2, 2];
                data[3, 2, 2] = data[4, 2, 0];
                data[4, 2, 0] = temp;

                temp = data[1, 0, 1];
                data[1, 0, 1] = data[2, 1, 2];
                data[2, 1, 2] = data[3, 2, 1];
                data[3, 2, 1] = data[4, 1, 0];
                data[4, 1, 0] = temp;

                temp = data[1, 0, 2];
                data[1, 0, 2] = data[2, 2, 2];
                data[2, 2, 2] = data[3, 2, 0];
                data[3, 2, 0] = data[4, 0, 0];
                data[4, 0, 0] = temp;
            }
            else if (side == "BI")
            {
                face = 5;

                temp = data[face, 0, 0];
                data[face, 0, 0] = data[face, 0, 2];
                data[face, 0, 2] = data[face, 2, 2];
                data[face, 2, 2] = data[face, 2, 0];
                data[face, 2, 0] = temp;

                temp = data[face, 0, 1];
                data[face, 0, 1] = data[face, 1, 2];
                data[face, 1, 2] = data[face, 2, 1];
                data[face, 2, 1] = data[face, 1, 0];
                data[face, 1, 0] = temp;

                temp = data[1, 0, 0];
                data[1, 0, 0] = data[4, 2, 0];
                data[4, 2, 0] = data[3, 2, 2];
                data[3, 2, 2] = data[2, 0, 2];
                data[2, 0, 2] = temp;

                temp = data[1, 0, 1];
                data[1, 0, 1] = data[4, 1, 0];
                data[4, 1, 0] = data[3, 2, 1];
                data[3, 2, 1] = data[2, 1, 2];
                data[2, 1, 2] = temp;

                temp = data[1, 0, 2];
                data[1, 0, 2] = data[4, 0, 0];
                data[4, 0, 0] = data[3, 2, 0];
                data[3, 2, 0] = data[2, 2, 2];
                data[2, 2, 2] = temp;
            }

            moves.Add(side);
        }

        public void solveCross()
        {
            while (crossSolved != true)
            {
                if (data[0, 1, 0] == 0 &&
                    data[0, 1, 2] == 0 &&
                    data[0, 0, 1] == 0 &&
                    data[0, 2, 1] == 0)
                {
                    crossSolved = true;
                    Console.WriteLine("Cross complete");
                }
                else
                {
                    while (data[0, 1, 2] == 0)
                    {
                        turn("F");
                    }

                    int counter = 0;

                    if (data[1, 0, 1] == 0 ||
                        data[1, 1, 0] == 0 ||
                        data[1, 1, 2] == 0 ||
                        data[1, 2, 1] == 0)
                    {
                        while (data[1, 1, 2] != 0)
                        {
                            turn("U");
                            counter++;
                        }
                        turn("RI");
                        if (counter > 0)
                        {
                            while (counter < 4)
                            {
                                turn("U");
                                counter++;
                            }
                        }
                    }

                    else if (data[2, 0, 1] == 0 ||
                        data[2, 1, 0] == 0 ||
                        data[2, 1, 2] == 0 ||
                        data[2, 2, 1] == 0)
                    {
                        while (data[2, 1, 2] != 0)
                        {
                            turn("R");
                            counter++;
                        }
                        turn("B");
                        turn("U");
                        turn("RI");
                        turn("UI");
                    }

                    else if (data[3, 0, 1] == 0 ||
                        data[3, 1, 0] == 0 ||
                        data[3, 1, 2] == 0 ||
                        data[3, 2, 1] == 0)
                    {
                        while (data[3, 1, 2] != 0)
                        {
                            turn("D");
                            counter++;
                        }
                        turn("R");
                        if (counter > 0)
                        {
                            while (counter < 4)
                            {
                                turn("D");
                                counter++;
                            }
                        }
                    }

                    else if (data[4, 0, 1] == 0 ||
                        data[4, 1, 0] == 0 ||
                        data[4, 1, 2] == 0 ||
                        data[4, 2, 1] == 0)
                    {
                        while (data[4, 1, 0] != 0)
                        {
                            turn("L");
                            counter++;
                        }
                        turn("BI");
                        turn("U");
                        turn("RI");
                        turn("UI");
                        if (counter > 0)
                        {
                            while (counter < 4)
                            {
                                turn("L");
                                counter++;
                            }
                        }
                    }

                    else if (data[5, 0, 1] == 0 ||
                        data[5, 1, 0] == 0 ||
                        data[5, 1, 2] == 0 ||
                        data[5, 2, 1] == 0)
                    {
                        while (data[5, 1, 0] != 0)
                        {
                            turn("B");
                            counter++;
                        }
                        turn("R");
                        turn("R");
                    }
                }
            }
        }

        public void allignCenters()
        {
            int numAlligned;
            if (data[1, 2, 1] == data[1, 1, 1] &&
               data[2, 1, 0] == data[2, 1, 1] &&
               data[3, 0, 1] == data[3, 1, 1] &&
               data[4, 1, 2] == data[4, 1, 1])
            {
                centersAlligned = true;
                Console.WriteLine("Centers alligned");
            }
            else
            {
                do
                {
                    numAlligned = 0;
                    if (data[1, 2, 1] == data[1, 1, 1])
                        numAlligned++;
                    if (data[2, 1, 0] == data[2, 1, 1])
                        numAlligned++;
                    if (data[3, 0, 1] == data[3, 1, 1])
                        numAlligned++;
                    if (data[4, 1, 2] == data[4, 1, 1])
                        numAlligned++;
                    if (numAlligned < 2)
                        turn("F");
                }
                while (numAlligned < 2);

                if (data[1, 2, 1] != data[1, 1, 1])
                {

                    turn("U");
                    turn("U");
                }
                if (data[2, 1, 0] != data[2, 1, 1])
                {
                    turn("R");
                    turn("R");
                }
                if (data[3, 0, 1] != data[3, 1, 1])
                {
                    turn("D");
                    turn("D");
                }
                if (data[4, 1, 2] != data[4, 1, 1])
                {
                    turn("L");
                    turn("L");
                }

                if (data[0, 0, 1] != data[0, 1, 1])
                {
                    while (data[1, 0, 1] != data[1, 1, 1] || data[5, 0, 1] != data[0, 1, 1])
                    {
                        turn("B");
                    }
                    turn("U");
                    turn("U");
                }
                if (data[0, 1, 2] != data[0, 1, 1])
                {
                    while (data[2, 1, 2] != data[2, 1, 1] || data[5, 1, 0] != data[0, 1, 1])
                    {
                        turn("B");
                    }
                    turn("R");
                    turn("R");
                }
                if (data[0, 2, 1] != data[0, 1, 1])
                {
                    while (data[3, 2, 1] != data[3, 1, 1] || data[5, 2, 1] != data[0, 1, 1])
                    {
                        turn("B");
                    }
                    turn("D");
                    turn("D");
                }
                if (data[0, 1, 0] != data[0, 1, 1])
                {
                    while (data[4, 1, 0] != data[4, 1, 1] || data[5, 1, 2] != data[0, 1, 1])
                    {
                        turn("B");
                    }
                    turn("L");
                    turn("L");
                }

            }
            centersAlligned = true;
            Console.WriteLine("Centers alligned");
        }

        public void finishFirstLayer()
        {
            while (firstLayer != true)
            {
                if (data[0, 0, 0] == 0 &&
                    data[0, 0, 2] == 0 &&
                    data[0, 2, 0] == 0 &&
                    data[0, 2, 2] == 0 &&
                    data[1, 2, 0] == 1 &&
                    data[1, 2, 2] == 1 &&
                    data[2, 0, 0] == 2 &&
                    data[2, 2, 0] == 2 &&
                    data[3, 0, 0] == 3 &&
                    data[3, 0, 2] == 3 &&
                    data[4, 0, 2] == 4 &&
                    data[4, 2, 2] == 4)
                {
                    firstLayer = true;
                    Console.WriteLine("First layer done");
                }

                else if (data[1, 0, 0] == 0 ||
                        data[1, 0, 2] == 0 ||
                        data[2, 0, 2] == 0 ||
                        data[2, 2, 2] == 0 ||
                        data[3, 2, 0] == 0 ||
                        data[3, 2, 2] == 0 ||
                        data[4, 0, 0] == 0 ||
                        data[4, 2, 0] == 0)
                {
                    if (data[1, 0, 0] == 0 && data[5, 0, 2] == 1)
                    {
                        turn("U");
                        turn("B");
                        turn("UI");
                    }

                    else if (data[1, 0, 2] == 0 && data[5, 0, 0] == 1)
                    {
                        turn("UI");
                        turn("BI");
                        turn("U");
                    }

                    else if (data[2, 0, 2] == 0 && data[5, 0, 0] == 2)
                    {
                        turn("R");
                        turn("B");
                        turn("RI");
                    }

                    else if (data[2, 2, 2] == 0 && data[5, 2, 0] == 2)
                    {
                        turn("RI");
                        turn("BI");
                        turn("R");
                    }

                    else if (data[3, 2, 0] == 0 && data[5, 2, 2] == 3)
                    {
                        turn("DI");
                        turn("BI");
                        turn("D");
                    }

                    else if (data[3, 2, 2] == 0 && data[5, 2, 0] == 3)
                    {
                        turn("D");
                        turn("B");
                        turn("DI");
                    }

                    else if (data[4, 0, 0] == 0 && data[5, 0, 2] == 4)
                    {
                        turn("LI");
                        turn("BI");
                        turn("L");
                    }

                    else if (data[4, 2, 0] == 0 && data[5, 2, 2] == 4)
                    {
                        turn("L");
                        turn("B");
                        turn("LI");
                    }

                    else
                    {
                        turn("B");
                        Console.WriteLine("or here");
                    }

                }

                else if (data[1, 2, 0] == 0 || data[4, 0, 2] == 0 || data[5, 0, 2] == 0 ||
                        (data[0, 0, 0] == 0 && data[1, 2, 0] != 1) || (data[0, 0, 0] == 0 && data[4, 0, 2] != 4))
                {
                    turn("LI");
                    turn("BI");
                    turn("L");
                }

                else if (data[1, 2, 2] == 0 || data[2, 0, 0] == 0 || data[5, 0, 0] == 0 ||
                        (data[0, 0, 2] == 0 && data[1, 2, 2] != 1) || (data[0, 0, 2] == 0 && data[2, 0, 0] != 2))
                {
                    turn("R");
                    turn("B");
                    turn("RI");
                }

                else if (data[4, 2, 2] == 0 || data[3, 0, 0] == 0 || data[5, 2, 2] == 0 ||
                        (data[0, 2, 0] == 0 && data[3, 0, 0] != 3) || (data[0, 2, 0] == 0 && data[4, 2, 2] != 4))
                {
                    turn("L");
                    turn("B");
                    turn("LI");
                }

                else if (data[2, 2, 0] == 0 || data[3, 0, 2] == 0 || data[5, 2, 0] == 0 ||
                        (data[0, 2, 2] == 0 && data[2, 2, 0] != 2) || (data[0, 2, 2] == 0 && data[3, 0, 2] != 3))
                {
                    turn("RI");
                    turn("BI");
                    turn("R");
                }
            }
        }

        public void finishSecondLayer()
        {
            while (secondLayer != true)
            {
                if (data[1, 1, 0] == 1 &&
                   data[1, 1, 2] == 1 &&
                   data[2, 0, 1] == 2 &&
                   data[2, 2, 1] == 2 &&
                   data[3, 1, 0] == 3 &&
                   data[3, 1, 2] == 3 &&
                   data[4, 0, 1] == 4 &&
                   data[4, 2, 1] == 4)
                {
                    secondLayer = true;
                    Console.WriteLine("Second layer done");
                }

                else if ((data[5, 0, 1] != 5 && data[1, 0, 1] != 5) ||
                         (data[5, 1, 0] != 5 && data[2, 1, 2] != 5) ||
                         (data[5, 1, 2] != 5 && data[4, 1, 0] != 5) ||
                         (data[5, 2, 1] != 5 && data[3, 2, 1] != 5))
                {
                    if (data[1, 0, 1] == 1 && data[5, 0, 1] == 2)
                    {
                        turn("B");
                        turn("R");
                        turn("BI");
                        turn("RI");
                        turn("BI");
                        turn("UI");
                        turn("B");
                        turn("U");
                    }

                    else if (data[1, 0, 1] == 1 && data[5, 0, 1] == 4)
                    {
                        turn("BI");
                        turn("LI");
                        turn("B");
                        turn("L");
                        turn("B");
                        turn("U");
                        turn("BI");
                        turn("UI");
                    }

                    else if (data[2, 1, 2] == 2 && data[5, 1, 0] == 1)
                    {
                        turn("BI");
                        turn("UI");
                        turn("B");
                        turn("U");
                        turn("B");
                        turn("R");
                        turn("BI");
                        turn("RI");
                    }

                    else if (data[2, 1, 2] == 2 && data[5, 1, 0] == 3)
                    {
                        turn("B");
                        turn("D");
                        turn("BI");
                        turn("DI");
                        turn("BI");
                        turn("RI");
                        turn("B");
                        turn("R");
                    }

                    else if (data[3, 2, 1] == 3 && data[5, 2, 1] == 2)
                    {
                        turn("BI");
                        turn("RI");
                        turn("B");
                        turn("R");
                        turn("B");
                        turn("D");
                        turn("BI");
                        turn("DI");
                    }

                    else if (data[3, 2, 1] == 3 && data[5, 2, 1] == 4)
                    {
                        turn("B");
                        turn("L");
                        turn("BI");
                        turn("LI");
                        turn("BI");
                        turn("DI");
                        turn("B");
                        turn("D");
                    }

                    else if (data[4, 1, 0] == 4 && data[5, 1, 2] == 3)
                    {
                        turn("BI");
                        turn("DI");
                        turn("B");
                        turn("D");
                        turn("B");
                        turn("L");
                        turn("BI");
                        turn("LI");
                    }

                    else if (data[4, 1, 0] == 4 && data[5, 1, 2] == 1)
                    {
                        turn("B");
                        turn("U");
                        turn("BI");
                        turn("UI");
                        turn("BI");
                        turn("LI");
                        turn("B");
                        turn("L");
                    }

                    else
                    {
                        turn("B");
                        Console.WriteLine("here");
                    }
                }

                else if (data[1, 1, 0] != 1 || data[4, 0, 1] != 4)
                {
                    turn("BI");
                    turn("LI");
                    turn("B");
                    turn("L");
                    turn("B");
                    turn("U");
                    turn("BI");
                    turn("UI");
                }

                else if (data[1, 1, 2] != 1 || data[2, 0, 1] != 2)
                {
                    turn("B");
                    turn("R");
                    turn("BI");
                    turn("RI");
                    turn("BI");
                    turn("UI");
                    turn("B");
                    turn("U");
                }

                else if (data[2, 2, 1] != 2 || data[3, 1, 2] != 3)
                {
                    turn("B");
                    turn("D");
                    turn("BI");
                    turn("DI");
                    turn("BI");
                    turn("RI");
                    turn("B");
                    turn("R");
                }

                else if (data[3, 1, 0] != 3 || data[4, 2, 1] != 4)
                {
                    turn("BI");
                    turn("DI");
                    turn("B");
                    turn("D");
                    turn("B");
                    turn("L");
                    turn("BI");
                    turn("LI");
                }
            }
        }

        public void solveBackCross()
        {
            while (backCross != true)
            {
                int counter = 0;
                if (data[5, 0, 1] == 5)
                    counter++;
                if (data[5, 1, 0] == 5)
                    counter++;
                if (data[5, 1, 2] == 5)
                    counter++;
                if (data[5, 2, 1] == 5)
                    counter++;
                if(counter == 4)
                {
                    backCross = true;
                    Console.WriteLine("Back cross done");
                }
                else if (counter < 2 || (data[5,1,2] == 5 && data[5,2,1] == 5))
                {
                    turn("U");
                    turn("B");
                    turn("R");
                    turn("BI");
                    turn("RI");
                    turn("UI");
                }
                else if ((data[5, 1, 2] == 5 && data[5, 1, 0] == 5))
                {
                    turn("U");
                    turn("R");
                    turn("B");
                    turn("RI");
                    turn("BI");
                    turn("UI");
                }
                else
                {
                    turn("B");
                }
            }
        }

        public void finishBackFace()
        {
            while (backFace != true)
            {
                if (data[5, 0, 0] == 5 &&
                   data[5, 0, 2] == 5 &&
                   data[5, 2, 0] == 5 &&
                   data[5, 2, 2] == 5)
                {
                    backFace = true;
                    Console.WriteLine("Back face done");
                }

                else
                {
                    int counter = 0;
                    if (data[5, 0, 0] == 5)
                        counter++;
                    if (data[5, 0, 2] == 5)
                        counter++;
                    if (data[5, 2, 0] == 5)
                        counter++;
                    if (data[5, 2, 2] == 5)
                        counter++;

                    if (counter == 0)
                    {
                        while (data[4, 0, 0] != 5)
                        {
                            turn("B");
                        }
                    }

                    else if (counter == 1)
                    {
                        while (data[5, 0, 2] != 5)
                        {
                            turn("B");
                        }
                    }

                    else if (counter >= 2)
                    {
                        while (data[1, 0, 0] != 5)
                        {
                            turn("B");
                        }
                    }

                    turn("R");
                    turn("B");
                    turn("RI");
                    turn("B");
                    turn("R");
                    turn("B");
                    turn("B");
                    turn("RI");
                }

            }
        }

        public void positionBackCorners()
        {
            while (backCorners != true)
            {
                if (data[1, 0, 0] == data[1, 0, 2] &&
                    data[2, 0, 2] == data[2, 2, 2] &&
                    data[3, 2, 0] == data[3, 2, 2] &&
                    data[4, 0, 0] == data[4, 2, 0])
                
                {
                    while ((data[1, 0, 0] == 1 &&
                            data[1, 0, 2] == 1 &&
                            data[2, 0, 2] == 2 &&
                            data[2, 2, 2] == 2 &&
                            data[3, 2, 0] == 3 &&
                            data[3, 2, 2] == 3 &&
                            data[4, 0, 0] == 4 &&
                            data[4, 2, 0] == 4) == false)
                    {
                        turn("B");
                    }
                    backCorners = true;
                    Console.WriteLine("Back corners positioned");
                }

                else
                {
                    int counter = 0;
                    while (counter < 2)
                    {
                        counter = 0;
                        if (data[1, 0, 0] == 1 && data[4, 0, 0] == 4)
                            counter++;
                        if (data[1, 0, 2] == 1 && data[2, 0, 2] == 2)
                            counter++;
                        if (data[2, 2, 2] == 2 && data[3, 2, 2] == 3)
                            counter++;
                        if (data[3, 2, 0] == 3 && data[4, 2, 0] == 4)
                            counter++;
                        if (counter < 2)
                        {
                            turn("B");
                        }
                    }

                    if ((data[2, 2, 2] == 2 && data[3, 2, 2] == 3) && (data[3, 2, 0] == 3 && data[4, 2, 0] == 4))
                    {
                        turn("RI");
                        turn("U");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("UI");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("R");
                        turn("BI");
                    }

                    else if ((data[2, 2, 2] == 2 && data[3, 2, 2] == 3) && (data[1, 0, 2] == 1 && data[2, 0, 2] == 2))
                    {
                        turn("BI");
                        turn("RI");
                        turn("U");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("UI");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("R");
                        turn("BI");
                    }

                    else if ((data[1, 0, 0] == 1 && data[4, 0, 0] == 4) && (data[1, 0, 2] == 1 && data[2, 0, 2] == 2))
                    {
                        turn("BI");
                        turn("BI");
                        turn("RI");
                        turn("U");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("UI");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("R");
                        turn("BI");
                    }

                    else if ((data[1, 0, 0] == 1 && data[4, 0, 0] == 4) && (data[3, 2, 0] == 3 && data[4, 2, 0] == 4))
                    {
                        turn("B");
                        turn("RI");
                        turn("U");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("UI");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("R");
                        turn("BI");
                    }

                    else
                    {
                        turn("RI");
                        turn("U");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("UI");
                        turn("RI");
                        turn("D");
                        turn("D");
                        turn("R");
                        turn("R");
                        turn("BI");
                    }
                }
            }
        }

        public void positionBackEdges()
        {
            while (backEdges != true)
            {
                if (data[1, 0, 1] == 1 &&
                   data[2, 1, 2] == 2 &&
                   data[3, 2, 1] == 3 &&
                   data[4, 1, 0] == 4)
                {
                    backEdges = true;
                    Console.WriteLine("Back edges positioned");
                }
                else
                {
                    int counter = 0;
                    if (data[1, 0, 1] == 1)
                        counter++;
                    if (data[2, 1, 2] == 2)
                        counter++;
                    if (data[3, 2, 1] == 3)
                        counter++;
                    if (data[4, 1, 0] == 4)
                        counter++;

                    if (counter == 0)
                    {
                        turn("U");
                        turn("U");
                        turn("B");
                        turn("L");
                        turn("RI");
                        turn("U");
                        turn("U");
                        turn("LI");
                        turn("R");
                        turn("B");
                        turn("U");
                        turn("U");
                        while ((data[1, 0, 1] == 1 ||
                                data[2, 1, 2] == 2 ||
                                data[3, 2, 1] == 3 ||
                                data[4, 1, 0] == 4) == false)
                            turn("B");
                    }

                    else if (data[1, 0, 1] == 4 && data[2, 1, 2] == 1 && data[4, 1, 0] == 2)
                    {
                        turn("U");
                        turn("U");
                        turn("B");
                        turn("L");
                        turn("RI");
                        turn("U");
                        turn("U");
                        turn("LI");
                        turn("R");
                        turn("B");
                        turn("U");
                        turn("U");
                        while ((data[1, 0, 1] == 1 ||
                                data[2, 1, 2] == 2 ||
                                data[3, 2, 1] == 3 ||
                                data[4, 1, 0] == 4) == false)
                            turn("B");
                    }

                    else if(data[1, 0, 1] == 3 && data[2, 1, 2] == 1 && data[3, 2, 1] == 2)
                    {
                        turn("B");
                        turn("U");
                        turn("U");
                        turn("B");
                        turn("L");
                        turn("RI");
                        turn("U");
                        turn("U");
                        turn("LI");
                        turn("R");
                        turn("B");
                        turn("U");
                        turn("U");
                        while ((data[1, 0, 1] == 1 ||
                                data[2, 1, 2] == 2 ||
                                data[3, 2, 1] == 3 ||
                                data[4, 1, 0] == 4) == false)
                            turn("B");
                    }

                    else if(data[2, 1, 2] == 4 && data[3, 2, 1] == 2 && data[4, 1, 0] == 3)
                    {
                        turn("B");
                        turn("B");
                        turn("U");
                        turn("U");
                        turn("B");
                        turn("L");
                        turn("RI");
                        turn("U");
                        turn("U");
                        turn("LI");
                        turn("R");
                        turn("B");
                        turn("U");
                        turn("U");
                        while ((data[1, 0, 1] == 1 ||
                                data[2, 1, 2] == 2 ||
                                data[3, 2, 1] == 3 ||
                                data[4, 1, 0] == 4) == false)
                            turn("B");
                    }

                    else if(data[1, 0, 1] == 4 && data[4, 1, 0] == 3 && data[3, 2, 1] == 1)
                    {
                        turn("BI");
                        turn("U");
                        turn("U");
                        turn("B");
                        turn("L");
                        turn("RI");
                        turn("U");
                        turn("U");
                        turn("LI");
                        turn("R");
                        turn("B");
                        turn("U");
                        turn("U");
                        while ((data[1, 0, 1] == 1 ||
                                data[2, 1, 2] == 2 ||
                                data[3, 2, 1] == 3 ||
                                data[4, 1, 0] == 4) == false)
                            turn("B");
                    }

                    
                    else
                    {
                        turn("U");
                        turn("U");
                        turn("BI");
                        turn("L");
                        turn("RI");
                        turn("U");
                        turn("U");
                        turn("LI");
                        turn("R");
                        turn("BI");
                        turn("U");
                        turn("U");
                        while ((data[1, 0, 1] == 1 ||
                                data[2, 1, 2] == 2 ||
                                data[3, 2, 1] == 3 ||
                                data[4, 1, 0] == 4) == false)
                        {
                            turn("B");
                        }
                    }
                }
            }
        }

        public void removeTriples()
        {
            int a = 0;
            while (a < moves.Count - 2)
            {
                if (moves.ElementAt(a) == moves.ElementAt(a + 1) && moves.ElementAt(a) == moves.ElementAt(a + 2))
                {
                    if (moves.ElementAt(a) == "F")
                    {
                        moves[a] = "FI";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "U")
                    {
                        moves[a] = "UI";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "R")
                    {
                        moves[a] = "RI";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "D")
                    {
                        moves[a] = "DI";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "L")
                    {
                        moves[a] = "LI";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "B")
                    {
                        moves[a] = "BI";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "TI")
                    {
                        moves[a] = "T";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "UI")
                    {
                        moves[a] = "U";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "RI")
                    {
                        moves[a] = "R";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "DI")
                    {
                        moves[a] = "D";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "LI")
                    {
                        moves[a] = "L";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    else if (moves.ElementAt(a) == "BI")
                    {
                        moves[a] = "B";
                        moves.RemoveAt(a + 1);
                        moves.RemoveAt(a + 1);
                    }
                    a++;
                }
            }
        }
    }
}























































///hi rohit