#region File Description
//-----------------------------------------------------------------------------
// RewardsScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Displays the rewards earned by the party, from a quest or combat.
    /// </summary>
    class RewardsScreen : GameScreen
    {
        public enum RewardScreenMode
        {
            Quest,
            Combat,
        };

        /// <summary>
        /// The mode of this screen.
        /// </summary>
        private RewardScreenMode mode;


        #region Rewards


        private int experienceReward;
        private int goldReward;
        private List<Gear> gearReward;


        #endregion


        #region Graphics content


        private Texture2D backTexture;
        private Texture2D selectIconTexture;
        private Texture2D lineTexture;
        private Texture2D scrollUpTexture;
        private Texture2D scrollDownTexture;
        private Texture2D fadeTexture;

        private Vector2 backgroundPosition;
        private Vector2 textPosition;
        private Vector2 iconPosition;
        private Vector2 linePosition;
        private Vector2 selectPosition;
        private Vector2 selectIconPosition;
        private Vector2 screenSize;
        private Vector2 titlePosition;
        private Vector2 scrollUpPosition;
        private Vector2 scrollDownPosition;
        private Vector2 xpAwardPosition;
        private Vector2 goldAwardPosition;
        private Vector2 itemAwardPosition;
        private Rectangle fadeDest;


        #endregion


        #region Dialog Text


        private string titleText;
        private readonly string selectString = "Continue";


        #endregion


        #region Scrollable List Data


        /// <summary>
        /// Starting index of the list to be displayed
        /// </summary>
        private int startIndex;

        /// <summary>
        /// Ending index of the list to be displayed
        /// </summary>
        private int endIndex;

        /// <summary>
        /// Maximum number of lines to draw in the screen
        /// </summary>
        private int maxLines;

        /// <summary>
        /// Vertical spacing between each line
        /// </summary>
        private float lineSpacing;


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new RewardsScreen object.
        /// </summary>
        public RewardsScreen(RewardScreenMode mode, int experienceReward,
            int goldReward, List<Gear> gearReward)
            : base()
        {
            this.IsPopup = true;
            this.mode = mode;
            this.experienceReward = experienceReward;
            this.goldReward = goldReward;
            this.gearReward = gearReward;

            maxLines = 3;
            lineSpacing = 74 * ScaledVector2.ScaleFactor;

            startIndex = 0;
            endIndex = maxLines;

            if (endIndex > gearReward.Count)
            {
                endIndex = gearReward.Count;
            }

            // play the appropriate music
            switch (mode)
            {
                case RewardScreenMode.Combat:
                    // play the combat-victory music
                    AudioManager.PushMusic("WinTheme",false);
                    break;

                case RewardScreenMode.Quest:
                    // play the quest-complete music
                    AudioManager.PushMusic("QuestComplete",false);
                    break;
            }

            this.Exiting += new EventHandler(RewardsScreen_Exiting);
        }


        void RewardsScreen_Exiting(object sender, EventArgs e)
        {
            AudioManager.PopMusic();
        }


        /// <summary>
        /// Load the graphics content from the content manager.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;
            backTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
            selectIconTexture =
                content.Load<Texture2D>(@"Textures\Buttons\rpgbtn");
            scrollUpTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollUp");
            scrollDownTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\ScrollDown");
            lineTexture =
                content.Load<Texture2D>(@"Textures\GameScreens\SeparationLine");
            fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            fadeDest = new Rectangle(viewport.X, viewport.Y, viewport.Width,
                viewport.Height);
            backgroundPosition.X = (viewport.Width - backTexture.Width * ScaledVector2.DrawFactor) / 2;
            backgroundPosition.Y = (viewport.Height - backTexture.Height * ScaledVector2.DrawFactor) / 2;

            screenSize = new Vector2(viewport.Width, viewport.Height);

            selectIconPosition.X = screenSize.X / 2 + 120 * ScaledVector2.ScaleFactor;
            selectIconPosition.Y = backgroundPosition.Y + 520f * ScaledVector2.ScaleFactor;
            selectPosition.X = selectIconPosition.X -
                Fonts.ButtonNamesFont.MeasureString(selectString).X - 10f * ScaledVector2.ScaleFactor;
            selectPosition.Y = backgroundPosition.Y + 530f * ScaledVector2.ScaleFactor;

            textPosition = backgroundPosition + ScaledVector2.GetScaledVector(335f, 320f);
            iconPosition = backgroundPosition + ScaledVector2.GetScaledVector(155f, 303f);
            linePosition = backgroundPosition + ScaledVector2.GetScaledVector(142f, 285f);

            scrollUpPosition = backgroundPosition + ScaledVector2.GetScaledVector(810f, 300f);
            scrollDownPosition = backgroundPosition + ScaledVector2.GetScaledVector(810f, 480f);

            xpAwardPosition = backgroundPosition + ScaledVector2.GetScaledVector(160f, 180f);
            goldAwardPosition = backgroundPosition + ScaledVector2.GetScaledVector(160f, 210f);
            itemAwardPosition = backgroundPosition + ScaledVector2.GetScaledVector(160f, 240f);
        }


        #endregion


        #region Updating


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {
            bool backClicked = false;

            if (InputManager.IsButtonClicked(new Rectangle(
                (int)selectIconPosition.X, (int)selectIconPosition.Y,
                selectIconTexture.Width, selectIconTexture.Height)))
            {
                backClicked = true;
            }

            // exit the screen
            if (backClicked ||
                InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
                // give the rewards to the party
                Session.Party.PartyGold += goldReward;
                foreach (Gear gear in gearReward)
                {
                    Session.Party.AddToInventory(gear, 1);
                }
                Session.Party.GiveExperience(experienceReward);
            }
            // Scroll up
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                if (startIndex > 0)
                {
                    startIndex--;
                    endIndex--;
                }
            }
            // Scroll down
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                if (startIndex < gearReward.Count - maxLines)
                {
                    endIndex++;
                    startIndex++;
                }
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        ///  Draw the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Vector2 currentIconPosition = iconPosition;
            Vector2 currentTextPosition = textPosition;
            Vector2 currentlinePosition = linePosition;

            switch (mode)
            {
                case RewardScreenMode.Quest:
                    titleText = "Quest Complete";
                    break;

                case RewardScreenMode.Combat:
                    titleText = "Combat Won";
                    break;
            }
            titlePosition.X = (screenSize.X -
                Fonts.HeaderFont.MeasureString(titleText).X) / 2;
            titlePosition.Y = backgroundPosition.Y + lineSpacing;

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // Draw the fading screen
            spriteBatch.Draw(fadeTexture, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height), null, Color.White, 0f,
                Vector2.Zero,ScaledVector2.DrawFactor,SpriteEffects.None,0f);

            // Draw the popup background
            spriteBatch.Draw(backTexture, backgroundPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw the title
            spriteBatch.DrawString(Fonts.HeaderFont, titleText, titlePosition,
                Fonts.TitleColor);

            // Draw the experience points awarded
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "XP Awarded :        " + experienceReward,
                xpAwardPosition, Fonts.CountColor);

            // Draw the gold points awarded
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Gold Awarded :      " + Fonts.GetGoldString(goldReward),
                goldAwardPosition, Fonts.CountColor);

            // Draw the items awarded
            spriteBatch.DrawString(Fonts.GearInfoFont, "Items Awarded :",
                itemAwardPosition, Fonts.CountColor);

            // Draw horizontal divider lines
            for (int i = 0; i <= maxLines; i++)
            {
                spriteBatch.Draw(lineTexture, currentlinePosition, null,Color.White,0f,
                    Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                currentlinePosition.Y += lineSpacing;
            }

            // Draw the item details
            for (int i = startIndex; i < endIndex; i++)
            {
                // Draw the item icon
                gearReward[i].DrawIcon(ScreenManager.SpriteBatch, currentIconPosition);

                // Draw the item name
                spriteBatch.DrawString(Fonts.GearInfoFont,
                    gearReward[i].Name, currentTextPosition, Fonts.CountColor);

                // Increment the position to the next line
                currentTextPosition.Y += lineSpacing;
                currentIconPosition.Y += lineSpacing;
            }
            // Draw the scroll buttons
            spriteBatch.Draw(scrollUpTexture, scrollUpPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
            spriteBatch.Draw(scrollDownTexture, scrollDownPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            // Draw the select button and its corresponding text
            spriteBatch.Draw(selectIconTexture, selectIconPosition,null, Color.White,0f,
                Vector2.Zero, ScaledVector2.DrawFactor, SpriteEffects.None, 0f);

            Vector2 selectFontPosition = Fonts.GetCenterPositionInButton(Fonts.ButtonNamesFont, selectString,
                new Rectangle((int)selectIconPosition.X, (int)selectIconPosition.Y,
                    selectIconTexture.Width, selectIconTexture.Height));


            spriteBatch.DrawString(Fonts.ButtonNamesFont, selectString, selectFontPosition,
                Color.White);
            spriteBatch.End();
        }


        #endregion
    }
}