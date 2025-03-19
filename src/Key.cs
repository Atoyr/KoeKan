namespace Medoz.KoeKan;

/// <summary>
/// HotKeyクラスの初期化時に指定する修飾キー
/// </summary>
public enum MOD_KEY : uint {
    ALT         = 0x0001,
    CONTROL     = 0x0002,
    SHIFT       = 0x0004,
    WIN         = 0x0008,
    NONE        = 0x4000,
}

public enum KEY : uint {
    NUM_0       = 0x30,
    NUM_1,
    NUM_2,
    NUM_3,
    NUM_4,
    NUM_5,
    NUM_6,
    NUM_7,
    NUM_8,
    NUM_9,
    KEY_A       = 0x41,
    KEY_B,
    KEY_C,
    KEY_D,
    KEY_E,
    KEY_F,
    KEY_G,
    KEY_H,
    KEY_I,
    KEY_J,
    KEY_K,
    KEY_L,
    KEY_M,
    KEY_N,
    KEY_O,
    KEY_P,
    KEY_Q,
    KEY_R,
    KEY_S,
    KEY_T,
    KEY_U,
    KEY_V,
    KEY_W,
    KEY_X,
    KEY_Y,
    KEY_Z,
    ENTER       = 0x0D,
    ESCAPE      = 0x1B,
    KEY_F1      = 0x70,
    KEY_F2,
    KEY_F3,
    KEY_F4,
    KEY_F5,
    KEY_F6,
    KEY_F7,
    KEY_F8,
    KEY_F9,
    KEY_F10,
    KEY_F11,
    KEY_F12,
    NONE        = 0x00,
}

public static class ModKeyExtension
{
    public static MOD_KEY GetModKey(string modKey)
        => modKey.ToUpper() switch {
            "ALT"           => MOD_KEY.ALT,
            "CONTROL"       => MOD_KEY.CONTROL,
            "SHIFT"         => MOD_KEY.SHIFT,
            "WIN"           => MOD_KEY.WIN,
            _               => MOD_KEY.NONE
        };

    public static uint ToUInt(this MOD_KEY modKey)
        => (uint)modKey;
}

public static class KeyExtension
{
    public static KEY GetKey(string key)
        => key.ToUpper() switch {
            "0"         => KEY.NUM_0,
            "1"         => KEY.NUM_1,
            "2"         => KEY.NUM_2,
            "3"         => KEY.NUM_3,
            "4"         => KEY.NUM_4,
            "5"         => KEY.NUM_5,
            "6"         => KEY.NUM_6,
            "7"         => KEY.NUM_7,
            "8"         => KEY.NUM_8,
            "9"         => KEY.NUM_9,
            "A"         => KEY.KEY_A,
            "B"         => KEY.KEY_B,
            "C"         => KEY.KEY_C,
            "D"         => KEY.KEY_D,
            "E"         => KEY.KEY_E,
            "F"         => KEY.KEY_F,
            "G"         => KEY.KEY_G,
            "H"         => KEY.KEY_H,
            "I"         => KEY.KEY_I,
            "J"         => KEY.KEY_J,
            "K"         => KEY.KEY_K,
            "L"         => KEY.KEY_L,
            "M"         => KEY.KEY_M,
            "N"         => KEY.KEY_N,
            "O"         => KEY.KEY_O,
            "P"         => KEY.KEY_P,
            "Q"         => KEY.KEY_Q,
            "R"         => KEY.KEY_R,
            "S"         => KEY.KEY_S,
            "T"         => KEY.KEY_T,
            "U"         => KEY.KEY_U,
            "V"         => KEY.KEY_V,
            "W"         => KEY.KEY_W,
            "X"         => KEY.KEY_X,
            "Y"         => KEY.KEY_Y,
            "Z"         => KEY.KEY_Z,
            "ENTER"     => KEY.ENTER,
            "ESCAPE"    => KEY.ESCAPE,
            "F1"        => KEY.KEY_F1,
            "F2"        => KEY.KEY_F2,
            "F3"        => KEY.KEY_F3,
            "F4"        => KEY.KEY_F4,
            "F5"        => KEY.KEY_F5,
            "F6"        => KEY.KEY_F6,
            "F7"        => KEY.KEY_F7,
            "F8"        => KEY.KEY_F8,
            "F9"        => KEY.KEY_F9,
            "F10"       => KEY.KEY_F10,
            "F11"       => KEY.KEY_F11,
            "F12"       => KEY.KEY_F12,
            _           => KEY.NONE,
    };

    public static uint ToUInt(this KEY key)
        => (uint)key;
}