using System;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using MSCLoader;

namespace ShowBoltSize
{
    public class ShowBoltSize : Mod
    {
        private static readonly float DISPLAY_TIME = 2f;

        private SettingRadioButtons _showBoltSizeInfo;
        private FsmFloat _boltSize, _wrenchSize;
        private FsmString _guiInteractString;
        private SizeShowType _sizeShowType;
        private bool _loaded;
        private float _showTime = -1f;
        private string _displayStr = "";

        public override string ID => "ShowBoltSize";
        public override string Name => "Show Bolt Size";
        public override string Author => "KinzyKenzie (originally by Lex, wolf_vx)";
        public override string Version => "3.1";
        public override string Description => "Show size info for bolts";

        public override void ModSettings() {
            //Settings settings = new Settings("ShowBoltSizeInfo", "Bolt Size Information Amount", 3, new Action(this.ReadSettings));
            //Settings.AddSlider(this, settings, 0, 3);

            _showBoltSizeInfo = modSettings.AddRadioButtons( "ShowBoltSizeInfo", "Bolt Size Information Amount", 2, ( value ) => { _sizeShowType = (SizeShowType) value; },
                "Off", "Direction", "Direction + Distance", "Exact Number" );
        }

        public override void OnLoad() {
            GameObject gameObject = GameObject.Find( "PLAYER/Pivot/AnimPivot/Camera/FPSCamera/SelectItem" );
            PlayMakerFSM component = gameObject.GetComponent<PlayMakerFSM>();
            _wrenchSize = component.Fsm.GetFsmFloat( "OldWrench" );
            FsmInject( gameObject, "Tools", new Action( LoadChecker ) );
            _guiInteractString = PlayMakerGlobals.Instance.Variables.FindFsmString( "GUIinteraction" );
        }

        public override void ModSettingsLoaded() {
            _sizeShowType = (SizeShowType) _showBoltSizeInfo.Value;
        }

        public override void Update() {
            if( _showTime < -0.9 )
                return;

            _showTime -= Time.deltaTime;

            if( _showTime <= 0.0 ) {
                _showTime = -1f;
                _displayStr = "";
            }

            _guiInteractString.Value = _displayStr;
        }

        private void LoadChecker() {
            if( _loaded )
                return;

            GameObject go = GameObject.Find( "PLAYER/Pivot/AnimPivot/Camera/FPSCamera/2Spanner/Raycast" );
            PlayMakerFSM[] components = go.GetComponents<PlayMakerFSM>();
            Fsm fsm = null;

            foreach( PlayMakerFSM playMakerFSM in components ) {
                if( playMakerFSM.FsmName == "Check" )
                    fsm = playMakerFSM.Fsm;
            }

            _boltSize = fsm.GetFsmFloat( "BoltSize" );
            FsmInject( go, "State 1", new Action( CheckBoltCallback ) );
            FsmInject( go, "State 2", new Action( CheckBoltCallback ) );
            _loaded = true;
        }

        private void CheckBoltCallback() {
            int sizeTool = Mathf.RoundToInt( _wrenchSize.Value * 10 );
            int sizeBolt = Mathf.RoundToInt( _boltSize.Value * 10 );

            if( sizeTool == sizeBolt )
                return;

            _showTime = DISPLAY_TIME;

            if( _sizeShowType == SizeShowType.ExactNumber ) {
                _displayStr = "Size " + sizeBolt;
                return;
            }

            if( _sizeShowType == SizeShowType.Direction ) {

                if( sizeBolt > sizeTool ) {
                    _displayStr = "Needs bigger tool";
                    return;
                }

                if( sizeBolt < sizeTool ) {
                    _displayStr = "Needs smaller tool";
                    return;
                }

            } else if( _sizeShowType == SizeShowType.DirectionDistance ) {

                string direction = ( sizeBolt > sizeTool ) ? "bigger " : "smaller ";
                int diff = Math.Abs( sizeBolt - sizeTool );
                string weight;

                if( diff == 1 )
                    weight = "slightly ";

                else if( diff < 4 )
                    weight = "";

                else
                    weight = "way ";

                _displayStr = "Needs " + weight + direction + "tool";
            }
        }

        public static bool FsmInject( GameObject obj, string stateName, Action callback ) {

            bool flag = false;
            PlayMakerFSM[] components = obj.GetComponents<PlayMakerFSM>();

            for( int i = 0; i < components.Length; i++ ) {
                FsmState[] states = components[ i ].Fsm.States;
                if( states != null ) {
                    foreach( FsmState fsmState in states ) {
                        if( fsmState != null && fsmState.Name == stateName ) {
                            flag = true;
                            fsmState.Actions = new List<FsmStateAction>( fsmState.Actions )
                            {
                                new FsmHook
                                {
                                    Call = callback
                                }
                            }.ToArray();
                        }
                    }
                }
            }

            if( !flag ) {
                ModConsole.Log( "Didn't find " + obj.name + " state " + stateName );
            }

            return flag;
        }
    }
}
