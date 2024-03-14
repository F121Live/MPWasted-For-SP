﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using GTA;
using GTA.Native;
using GTA.UI;
using GTA.Math;

namespace MPWasted
{

    public class NoSlowMotion : Script
    {
        float timeScale;

        int timer = 0;

        public static bool needsHospital;

        public NoSlowMotion()
        {
            Tick += OnTick;
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
        }

        void Respawn_Controller()
        {
            Function.Call(Hash.DISPLAY_HUD_WHEN_NOT_IN_STATE_OF_PLAY_THIS_FRAME);
            Function.Call(Hash.SHOW_HUD_COMPONENT_THIS_FRAME, 21);
            Function.Call(Hash.FORCE_GAME_STATE_PLAYING);
            Function.Call(Hash.IGNORE_NEXT_RESTART, true);
            Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, false);
            Hud.IsVisible = true;
            Hud.IsRadarVisible = true;
            Function.Call(Hash.DISPLAY_HUD, true);//Doesn't work here?
            Function.Call(Hash.DISPLAY_RADAR, true);//Doesn't work here, either.
            ForceTimeScale();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!needsHospital) // Show regular Wasted if needed.
                Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "respawn_controller");
            else // Set up MP Wasted.
            {
                Function.Call(Hash.IGNORE_NEXT_RESTART, true);
                Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, false);
                timeScale = Game.TimeScale;
                if (timeScale != 1f)
                    Game.TimeScale = 1f;
                Wait(100);
                if (timeScale != 1f)
                    Game.TimeScale = 1f;
                GTA.UI.Screen.StopEffects();
                Function.Call(Hash.FORCE_GAME_STATE_PLAYING);
                needsHospital = false;
            }
            Game.Player.Character.DropsEquippedWeaponOnDeath = false;
            if (Game.Player.IsDead || Game.Player.Character.Health == 0 && !needsHospital)
                Respawn_Controller();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void ForceTimeScale()
        {
            timeScale = Game.TimeScale;
            if (timeScale != 1f)
                Game.TimeScale = 1f;
        }

        public static void NeedsHospital(bool value)
        {
            needsHospital = value;
        }
    }

    public class MPWasted : Script
    {

        private bool alreadyPlayed = false;

        private bool playCamRepeat = true;

        bool isLoaded = false;

        bool showShard = false;

        string wasted;

        Scaleform movie;

        public MPWasted()
        {
            Tick += OnTick;
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
        }
        private void OnTick(object sender, EventArgs e)
        {
            if (!isLoaded && !Game.IsLoading)
            {
                LoadResources();
            }
            if (Game.Player.IsDead || Game.Player.Character.Health == 0)
            {
                ShowShard();
                if (playCamRepeat)
                {
                    playCamRepeat = false;
                    CameraOn();
                    PlayMPWasted();
                }
            }
            else
            {
                alreadyPlayed = false;
                CameraOff();
            }
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // if (e.KeyCode == Keys.NumPad4) // Debugging purposes
            // {
            //     CameraOn();
            //     PlayMPWasted();
            // }
            // if (e.KeyCode == Keys.NumPad5) // Debugging purposes
            // {
            //     CameraOff();
            //     showShard = false;
            // }
            // if (e.KeyCode == Keys.NumPad3) // Debugging purposes
            // {
            //     CameraOff();
            //     showShard = false;
            // }
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void LoadResources() // Load MP audio resources and Scaleform.
        {
            Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_ENTITY, Game.Player.Character);
            Function.Call(Hash.SET_AUDIO_FLAG, "LoadMPData", true);
            wasted = Game.GetLocalizedString("RESPAWN_W_MP"); // Gets white colour Wasted variant.
            movie = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
            Function.Call(Hash.REQUEST_SCRIPT_AUDIO_BANK, "mp_wasted", 1);
            isLoaded = true;
        }

        private void PlayMPWasted()
        {
            if (!alreadyPlayed && isLoaded)
            {
                alreadyPlayed = true;
                movie.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", wasted, "", 27); // "27" sets the color to Red, then the game automatically fades it back to white.
                Wait(735);
                int s = Audio.PlaySoundFrontend("MP_Flash", "WastedSounds");
                Wait(200);
                showShard = true;
            }
        }

        private void CameraOn()
        {
            GTA.UI.Screen.StartEffect(GTA.UI.ScreenEffect.DeathFailMpIn, 0, false);
            GTA.GameplayCamera.Shake(CameraShake.DeathFail, 1f);
        }

        private void CameraOff()
        {
            playCamRepeat = true;
            showShard = false;
            GTA.UI.Screen.StopEffect(GTA.UI.ScreenEffect.DeathFailMpIn);
            GTA.GameplayCamera.StopShaking();
        }

        private void ShowShard()
        {
            if (showShard)
            {
                movie.Render2D();
            }
        }
    }

    public class RespawnAction : Script
    {
        bool initialized = false;

        bool respawning = false;

        public Vector3 respawnpos = GTA.World.GetSafeCoordForPed(Game.Player.Character.Position, true, 16);

        public Vector3 oldSpawn = new Vector3();

        Vector3 lastPedPos = GTA.World.GetSafeCoordForPed(Game.Player.Character.Position, true, 16);

        float heading = 0f;

        Ped randomPed1;

        bool showHelpText;

        int type;

        int tick = 0;

        public RespawnAction()
        {
            Tick += OnTick;
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
        }

        private void OnTick(object sender, EventArgs e)
        {
            tick++;
            if (tick > 500 && Game.Player.IsAlive && !Game.IsLoading)
            {
                respawnpos = GetCoords();
                tick = 0;
            }
            if (showHelpText) // Debugging, ignore.
            {
                switch (type)
                {
                    case 1:
                        GTA.UI.Screen.ShowHelpTextThisFrame("Respawn pos = " + respawnpos.X.ToString() + "x " + respawnpos.Y.ToString() + "y " + respawnpos.Z.ToString(), true);
                        break;
                    case 2:
                        Vector3 mypos = Game.Player.Character.Position;
                        float heading = Function.Call<float>(Hash.GET_ENTITY_HEADING, Game.Player.Character);
                        GTA.UI.Screen.ShowHelpTextThisFrame("Player pos = " + mypos.X.ToString() + "x " + mypos.Y.ToString() + "y " + mypos.Z.ToString() + "z. heading: " + heading.ToString() + "f", true);
                        break;
                    default:
                        GTA.UI.Screen.ShowHelpTextThisFrame("Respawn pos = " + respawnpos.X.ToString() + "x " + respawnpos.Y.ToString() + "y " + respawnpos.Z.ToString(), true);
                        break;
                }
            }
            if (!initialized && !Game.IsLoading)
            {
                respawnpos = GetCoords();
                Function.Call(Hash.SET_AUDIO_FLAG, "LoadMPData", true);
                Function.Call(Hash.REQUEST_SCRIPT_AUDIO_BANK, "mp_wasted", 1);
                initialized = true;
            }
            if (Game.Player.IsDead || Game.Player.Character.Health == 0)
            {
                Wait(4720); // 4720
                if (!respawning)
                    Respawn();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // if (e.KeyCode == Keys.NumPad2)
            // {
            //     HideNotifications();
            // }
            // if (e.KeyCode == Keys.NumPad3)
            // {
            //     Respawn();
            // }
            // if (e.KeyCode == Keys.NumPad9)
            // {
            //     respawnpos = GetCoords();
            //     ShowNotifications();
            // }
            // if (e.KeyCode == Keys.NumPad8)
            // {
            //     ShowPlayerCoords();
            // }
            // if (e.KeyCode == Keys.NumPad6)
            // {
            //     float z = Function.Call<float>(Hash.GET_ENTITY_HEIGHT_ABOVE_GROUND, Game.Player.Character);
            //     Vector3 tp = Game.Player.Character.Position;
            //     if (z > 99f)
            //     {
            //         Function.Call(Hash.START_PLAYER_TELEPORT, Game.Player, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z - z, Game.Player.Character.Heading, true, true, true);
            //     }
            //     Vector3 spawn = GTA.World.GetSafeCoordForPed(tp, true, 16);
            //     Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, spawn.X, spawn.Y, spawn.Z + 0.2f, heading, false, false, false, false, false);
            // }
            // if (e.KeyCode == Keys.NumPad7)
            // {
            //     TestNode();
            // }
            // if (e.KeyCode == Keys.NumPad0)
            // {
            //     HideNotifications();
            // }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void Respawn()
        {
            respawnpos = GetCoords();
            if (!respawning)
            {
                GTA.UI.Screen.FadeOut(500);
                respawning = true;
                Wait(2500);
                AttemptRespawn();
            }
            else
            {
                AttemptRespawn();
            }
        }

        private Vector3 GetCoords()
        {
            Vector3 pos = new Vector3();
            pos = FindSpawnPoint();
            return pos;
        }

        private Vector3 FindSpawnPoint()
        {
            int i = 0;
            int range = 0;
            bool paviment;
            if (Game.Player.Character.Position.X >= 800) // Check if player is out of LS.
            {
                paviment = false; // If out of LS, there's no paviment to spawn in.
                // This confuses the game if set to true, as there's nothing considered paviment
                // at the north of the map.
            }
            else
            {
                paviment = true; // OMG pavmetn:)
            }
            Random rng = new Random();
            Vector3 coords = Game.Player.Character.Position;
            Vector3 spawnPoint = new Vector3(0f, 0f, 0f);
            Vector3 point;
            Vector3 rand = new Vector3(0, 0, 0);
            OutputArgument tempcoords = new OutputArgument();
            OutputArgument temproadheading = new OutputArgument();
            do
            {
                i = i + 20;
                rand.X = rng.Next(-160, 160);
                rand.Y = rng.Next(-180, 180);
                rand.Z = rng.Next(-5, 50);
                range = rng.Next(50, 180);
                coords = Game.Player.Character.Position;
                point = Function.Call<Vector3>(Hash.FIND_SPAWN_POINT_IN_DIRECTION, coords.X + rand.X, coords.Y + rand.Y, coords.Z + rand.Z, 0, 0, 0, range, tempcoords);
                spawnPoint = GTA.World.GetSafeCoordForPed(tempcoords.GetResult<Vector3>(), paviment, 0);
                Wait(5); // We wait or else the game stutters.
                if (spawnPoint.X != 0f && spawnPoint.Y != 0f && spawnPoint.Z != 0f)
                {
                    i = 0;
                    Function.Call<Vector3>(Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, spawnPoint.X, spawnPoint.Y, spawnPoint.Z, tempcoords, temproadheading, 1, 3, 0);
                    heading = temproadheading.GetResult<float>() + 90f;
                    oldSpawn = spawnPoint;
                    return spawnPoint;
                }
                if (i > 200)
                {
                    i = 0;
                    break;
                }
            }
            while (spawnPoint.X == 0f && spawnPoint.Y == 0f && spawnPoint.Z == 0f);
            return oldSpawn;
        }

        private void AttemptRespawn()
        {
            TimeSpan time = World.CurrentTimeOfDay;
            GTA.UI.Screen.StopEffect(GTA.UI.ScreenEffect.DeathFailMpIn);
            GTA.GameplayCamera.StopShaking();
            Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_ENTITY, Game.Player.Character);
            if (respawnpos.X != 0f && respawnpos.Y != 0f && respawnpos.Z != 0f)
            {
                Function.Call(Hash.LOAD_SCENE, respawnpos.X, respawnpos.Y, respawnpos.Z);
                Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, respawnpos.X, respawnpos.Y, respawnpos.Z, heading, false, false, false, false, false);
                Function.Call(Hash.CLEAR_AREA, respawnpos.X, respawnpos.Y, respawnpos.Z, 6000, false, false, false, false);
                Function.Call(Hash.FORCE_GAME_STATE_PLAYING);
                World.CurrentTimeOfDay = time;
                Wait(100);
                GTA.UI.Screen.FadeIn(500);
                respawning = false;
            }
            else
            {
                respawning = true;
                Softlock(); // Should never happen.
            }
        }

        private void Softlock()
        {
            GTA.UI.Screen.FadeIn(600);
            GTA.UI.Notification.Show(GTA.UI.NotificationIcon.SocialClub, "Debug", "ScriptHookDotNet", "Funny softlock, restart the game or force revive.", false, false);
        }

        private void ShowNotifications() // Debugging.
        {
            showHelpText = false;
            type = 1;
            // if (randomPed1.IsAlive)
            //     hash = randomPed1.Model.GetHashCode();
            // else
            //     hash = 0;
            showHelpText = true;
        }

        private void ShowPlayerCoords() // Debugging.
        {
            showHelpText = false;
            type = 2;
            showHelpText = true;
            //notification = GTA.UI.Notification.Show(GTA.UI.NotificationIcon.SocialClub, "Debug", "ScriptHookDotNet", "Player pos = " + mypos.X.ToString() + "x " + mypos.Y.ToString() + "y " + mypos.Z.ToString() + "z. heading: " + heading.ToString() + "f", false, false);
        }

        private void HideNotifications() // Debugging.
        {
            showHelpText = false;
        }
    }
}
