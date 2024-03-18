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
            Function.Call(Hash.SET_RADAR_AS_EXTERIOR_THIS_FRAME);//Doesn't work here, either.
            Function.Call(Hash.DISPLAY_RADAR, true);//Doesn't work here, either.
            //Function.Call(Hash.SHOW_HUD_COMPONENT_THIS_FRAME, 21);
            //Function.Call(Hash.SHOW_HUD_COMPONENT_THIS_FRAME, 18);
            Function.Call(Hash.FORCE_GAME_STATE_PLAYING);
            Function.Call(Hash.IGNORE_NEXT_RESTART, true);
            Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, false);
            //Function.Call(Hash.DISPLAY_HUD, true);//Doesn't work here?
            //Function.Call(Hash.DISPLAY_RADAR, true);//Doesn't work here, either.
            //Hud.IsVisible = true;
            //Hud.IsRadarVisible = true;
            Game.Player.Character.DropsEquippedWeaponOnDeath = false;
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
                if (timeScale != 1f)
                    Game.TimeScale = 1f;
                GTA.UI.Screen.StopEffects();
                Function.Call(Hash.FORCE_GAME_STATE_PLAYING);
                needsHospital = false;
            }
            if (Game.Player.Character.Health <= 0 && !needsHospital)
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

        private bool playedWastedSounds = false;

        private bool playedPart2 = false;

        private bool playCamRepeat = true;

        public static bool isLoaded = false;

        public MPWasted()
        {
            Tick += OnTick;
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
            LoadResources();
        }
        private void OnTick(object sender, EventArgs e)
        {
            if (Game.Player.Character.Health <= 0)
            {
                if (!isLoaded && !Game.IsLoading)
                {
                    LoadResources();
                }
                if (playCamRepeat)
                {
                    playCamRepeat = false;
                    CameraOn();
                    PlayMPWasted();
                }
            }
            else
            {
                playedWastedSounds = false;
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

        public static void LoadResources() // Load MP audio resources and Scaleform.
        {
            Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_ENTITY, Game.Player.Character);
            Function.Call(Hash.SET_AUDIO_FLAG, "LoadMPData", true);
            Function.Call(Hash.REQUEST_SCRIPT_AUDIO_BANK, "mp_wasted", 1);
            ShardManager.LoadShard();
            isLoaded = true;
        }

        private void PlayMPWasted()
        {
            if (!playedWastedSounds && isLoaded)
            {
                playedWastedSounds = true;
                Wait(750);
                int s = Audio.PlaySoundFrontend("MP_Flash", "WastedSounds");
                Wait(1000);
                int s1 = Audio.PlaySoundFrontend("MP_Impact", "WastedSounds");
            }
        }

        private void CameraOn()
        {
            ShardManager.CallShard();
            GTA.UI.Screen.StartEffect(GTA.UI.ScreenEffect.DeathFailMpIn, 0, false);
            GTA.GameplayCamera.Shake(CameraShake.DeathFail, 1f);

        }

        private void CameraOff()
        {
            playCamRepeat = true;
            GTA.UI.Screen.StopEffect(GTA.UI.ScreenEffect.DeathFailMpIn);
            GTA.GameplayCamera.StopShaking();
        }
    }

    public class ShardManager : Script
    {

        bool showShard = false;

        public static string wasted;

        public static Scaleform movie;

        public static int tick = 0;

        public ShardManager()
        {
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Game.Player.Character.Health <= 0)
                ShowShard();
        }

        public static void LoadShard()
        {
            wasted = Game.GetLocalizedString("RESPAWN_W_MP"); // Gets white colour Wasted variant.
            movie = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
        }

        public static void CallShard()
        {
            tick = 0;
            movie.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", wasted, "", 27); // "27" sets the color to Red, then the game automatically fades it back to white.
        }

        private void ShowShard()
        {
            tick++;
            if (tick > 110)
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
            if (!initialized && !Game.IsLoading)
            {
                NoSlowMotion.NeedsHospital(false);
                respawnpos = GetCoords();
                MPWasted.LoadResources();
                initialized = true;
            }
            if (tick > 500 && Game.Player.IsAlive && !Game.IsLoading)
            {
                respawnpos = GetCoords();
                tick = 0;
            }
            if (Game.Player.Character.Health <= 0)
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
                Wait(900);
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
                rand.X = rng.Next(-160 - i, 160 + i);
                rand.Y = rng.Next(-180 - i, 180 + i);
                rand.Z = rng.Next(-15, 50);
                range = rng.Next(50, 180 + i);
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
                if (i > 1200)
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
