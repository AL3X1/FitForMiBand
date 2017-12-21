Public Class CustomBluetoothProfile

    Public Class Generic
        Public Shared service As Guid = New Guid("00001800-0000-1000-8000-00805F9B34FB")
        Public Shared devicenameCharacteristic As Guid = New Guid("00002A00-0000-1000-8000-00805F9B34FB")
    End Class

    Public Class Information
        Public Shared service As Guid = New Guid("0000180A-0000-1000-8000-00805F9B34FB")
        Public Shared softwareCharacteristic As Guid = New Guid("00002A28-0000-1000-8000-00805F9B34FB")
    End Class

    Public Class Basic
        Public Shared service As Guid = New Guid("0000fee0-0000-1000-8000-00805f9b34fb")
        Public Shared timeCharacterisic As Guid = New Guid("00002A2B-0000-1000-8000-00805F9B34FB")
        Public Shared configurationCharacteristic As Guid = New Guid("00000003-0000-3512-2118-0009AF100700")
        Public Shared usersettingsCharacteristic As Guid = New Guid("00000008-0000-3512-2118-0009AF100700")

        Private Shared ENDPOINT_DISPLAY As Byte = 6

        Public Shared COMMAND_ENABLE_DISPLAY_ON_LIFT_WRIST As Byte() = New Byte() {ENDPOINT_DISPLAY, 5, 0, 1}
        Public Shared COMMAND_DISABLE_DISPLAY_ON_LIFT_WRIST As Byte() = New Byte() {ENDPOINT_DISPLAY, 5, 0, 0}
        Public Shared WEAR_LOCATION_LEFT_WRIST As Byte() = New Byte() {32, 0, 0, 2}
        Public Shared WEAR_LOCATION_RIGHT_WRIST As Byte() = New Byte() {32, 0, 0, 130}

        Public Shared COMMAND_SET_USERINFO As Byte = 79

        Public Shared DATEFORMAT_DATE_TIME As Byte() = New Byte() {ENDPOINT_DISPLAY, 10, 0, 3}
        Public Shared DATEFORMAT_TIME As Byte() = New Byte() {ENDPOINT_DISPLAY, 10, 0, 0}
        Public Shared DATEFORMAT_TIME_12_HOURS As Byte() = New Byte() {ENDPOINT_DISPLAY, 2, 0, 0}
        Public Shared DATEFORMAT_TIME_24_HOURS As Byte() = New Byte() {ENDPOINT_DISPLAY, 2, 0, 1}
        Public Shared COMMAND_ENABLE_GOAL_NOTIFICATION As Byte() = New Byte() {ENDPOINT_DISPLAY, 6, 0, 1}
        Public Shared COMMAND_DISABLE_GOAL_NOTIFICATION As Byte() = New Byte() {ENDPOINT_DISPLAY, 6, 0, 0}
        Public Shared COMMAND_ENABLE_ROTATE_WRIST_TO_SWITCH_INFO As Byte() = New Byte() {ENDPOINT_DISPLAY, 13, 0, 1}
        Public Shared COMMAND_DISABLE_ROTATE_WRIST_TO_SWITCH_INFO As Byte() = New Byte() {ENDPOINT_DISPLAY, 13, 0, 0}
        Public Shared COMMAND_ENABLE_DISPLAY_CALLER As Byte() = New Byte() {ENDPOINT_DISPLAY, 16, 0, 0, 1}
        Public Shared COMMAND_DISABLE_DISPLAY_CALLER As Byte() = New Byte() {ENDPOINT_DISPLAY, 16, 0, 0, 0}
        Public Shared COMMAND_DISTANCE_UNIT_METRIC As Byte() = New Byte() {ENDPOINT_DISPLAY, 3, 0, 0}
        Public Shared COMMAND_DISTANCE_UNIT_IMPERIAL As Byte() = New Byte() {ENDPOINT_DISPLAY, 3, 0, 1}

        Public Shared ENDPOINT_DND As Byte = 9
        Public Shared COMMAND_DO_NOT_DISTURB_AUTOMATIC As Byte() = New Byte() {ENDPOINT_DND, 131}
        Public Shared COMMAND_DO_NOT_DISTURB_OFF As Byte() = New Byte() {ENDPOINT_DND, 130}
        Public Shared COMMAND_DO_NOT_DISTURB_SCHEDULED As Byte() = New Byte() {ENDPOINT_DND, 129, 1, 0, 6, 0}

        ' The 4 last bytes set the start And end time in 24h format
        Public Shared DND_BYTE_START_HOURS As Byte = 2
        Public Shared DND_BYTE_START_MINUTES As Byte = 3
        Public Shared DND_BYTE_END_HOURS As Byte = 4
        Public Shared DND_BYTE_END_MINUTES As Byte = 5

        Public Shared ENDPOINT_DISPLAY_ITEMS As Byte = 10

        Public Shared DISPLAY_ITEM_BIT_CLOCK As Byte = 1
        Public Shared DISPLAY_ITEM_BIT_STEPS As Byte = 2
        Public Shared DISPLAY_ITEM_BIT_DISTANCE As Byte = 4
        Public Shared DISPLAY_ITEM_BIT_CALORIES As Byte = 8
        Public Shared DISPLAY_ITEM_BIT_HEART_RATE As Byte = 16
        Public Shared DISPLAY_ITEM_BIT_BATTERY As Byte = 32

        ' Second byte must be a bitwise Or combination of the above
        ' The clock can't be disabled
        Public Shared SCREEN_CHANGE_BYTE As Byte = 1
        Public Shared COMMAND_CHANGE_SCREENS As Byte() = New Byte() {ENDPOINT_DISPLAY_ITEMS, DISPLAY_ITEM_BIT_CLOCK, 0, 0, 1, 2, 3, 4, 5}

    End Class

    Public Class AlertLevel
        Public Shared service As Guid = New Guid("00001802-0000-1000-8000-00805f9b34fb")
        Public Shared alertLevelCharacteristic As Guid = New Guid("00002a06-0000-1000-8000-00805f9b34fb")
    End Class

    Public Class NofiticationService
        Public Shared service As Guid = New Guid("00001811-0000-1000-8000-00805F9B34FB")
        Public Shared newAlertCharacteristic As Guid = New Guid("00002A46-0000-1000-8000-00805F9B34FB")

        Public Const ALERT_LEVEL_NONE As Integer = 0
        Public Const ALERT_LEVEL_MESSAGE As Integer = 1
        Public Const ALERT_LEVEL_PHONE_CALL As Integer = 2
        Public Const ALERT_LEVEL_VIBRATE_ONLY As Integer = 3
        Public Const ALERT_LEVEL_CUSTOM As Integer = 250 ' HEX-Value 0xfa

        Public Const ICON_CHAT As Integer = 0
        Public Const ICON_PENGUIN As Integer = 1
        Public Const ICON_CHAT_MI As Integer = 2
        Public Const ICON_FB As Integer = 3
        Public Const ICON_TWITTER As Integer = 4
        Public Const ICON_MIBAND As Integer = 5
        Public Const ICON_SNAPCHAT As Integer = 6
        Public Const ICON_WHATSAPP As Integer = 7
        Public Const ICON_MANTA As Integer = 8
        Public Const ICON_XX0 As Integer = 9
        Public Const ICON_ALARM As Integer = 16
        Public Const ICON_SHATTERED_GLASS As Integer = 17
        Public Const ICON_INSTAGRAM As Integer = 18
        Public Const ICON_CHAT_GHOST As Integer = 19
        Public Const ICON_COW As Integer = 20
        Public Const ICON_XX2 As Integer = 21
        Public Const ICON_XX3 As Integer = 22
        Public Const ICON_XX4 As Integer = 23
        Public Const ICON_XX5 As Integer = 24
        Public Const ICON_XX6 As Integer = 25
        Public Const ICON_EGALE As Integer = 26
        Public Const ICON_CALENDAR As Integer = 27
        Public Const ICON_XX7 As Integer = 28
        Public Const ICON_PHONE_CALL As Integer = 29
        Public Const ICON_CHAT_LINE As Integer = 30
        Public Const ICON_TELEGRAM As Integer = 31
        Public Const ICON_CHAT_TALK As Integer = 32
        Public Const ICON_SKYPE As Integer = 33
        Public Const ICON_VK As Integer = 34
        Public Const ICON_CIRCLES As Integer = 35
        Public Const ICON_HANGOUTS As Integer = 36
        Public Const ICON_MI As Integer = 37

    End Class

    Public Class HeartRate
        Public Shared service As Guid = New Guid("0000180d-0000-1000-8000-00805f9b34fb")
        Public Shared measurementCharacteristic As Guid = New Guid("00002a37-0000-1000-8000-00805f9b34fb")
        Public Shared descriptor As Guid = New Guid("00002902-0000-1000-8000-00805f9b34fb")
        Public Shared controlCharacteristic As Guid = New Guid("00002a39-0000-1000-8000-00805f9b34fb")
    End Class

    Public Class Authentication
        Public Shared service As Guid = New Guid("0000FEE1-0000-1000-8000-00805F9B34FB")
        Public Shared authCharacteristic As Guid = New Guid("00000009-0000-3512-2118-0009af100700")

        Public Const AUTH_SEND_KEY As Integer = 1 'HEX Value 0x01
        Public Const AUTH_REQUEST_RANDOM_AUTH_NUMBER As Integer = 2 'HEX Value 0x02
        Public Const AUTH_SEND_ENCRYPTED_AUTH_NUMBER As Integer = 3 'HEX Value 0x03
        Public Const AUTH_RESPONSE As Integer = 16 'HEX Value 0x10
        Public Const AUTH_SUCCESS As Integer = 1 'HEX Value 0x01
        Public Const AUTH_FAIL As Integer = 4 'HEX Value 0x04
        Public Const AUTH_BYTE As Integer = 8 'HEX Value 0x08

    End Class

End Class
