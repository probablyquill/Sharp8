using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace SharpC8
{
    public class GameHandler : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Chip8 chip8 = new Chip8();
        

        public GameHandler()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 50.0f);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            string fileName = "C:\\Users\\Elijah\\Documents\\programming\\SharpC8\\logo.ch8";
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long fileLength = new FileInfo(fileName).Length;
            byte[] rom = br.ReadBytes((int) fileLength);

            Console.WriteLine(rom);
            fs.Close();
            br.Close();

            chip8.Initialize();
            chip8.LoadGame(rom);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);


            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            _graphics.SynchronizeWithVerticalRetrace = false;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            chip8.EmulateCycle();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            Rectangle rect;
            Texture2D colorPixel = new Texture2D(GraphicsDevice, 1, 1);

            int multi = 10;
            _spriteBatch.Begin();
            Console.WriteLine(chip8.gfx.Length);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    rect = new Rectangle(x * multi, y * multi, multi, multi);
                    if (chip8.gfx[x + (y * 64)] == 0)
                    {
                        colorPixel.SetData<Color>(new Color[] { Color.White });
                        _spriteBatch.Draw(colorPixel, rect, Color.White);
                    }
                    else
                    {
                        colorPixel.SetData<Color>(new Color[] { Color.White });
                        _spriteBatch.Draw(colorPixel, rect, Color.Black);
                    }
                }
            }
            _spriteBatch.End();
            chip8.drawFlag = false;
            base.Draw(gameTime);
        }
    }
}
