package com.unity3d.unityconnect.plugins;
import android.content.Context;
import android.content.Intent;
import android.media.AudioManager;
import android.provider.Settings;
import android.support.v4.app.NotificationManagerCompat;

import com.unity3d.unityconnect.PickImageActivity;

public class CommonPlugin {

    public static Context mContext;

    public static void pauseAudioSession(){

        AudioManager.OnAudioFocusChangeListener afChangeListener = new AudioManager.OnAudioFocusChangeListener() {
            public void onAudioFocusChange(int focusChange) {
                switch (focusChange) {
                    //重新获取焦点
                    case AudioManager.AUDIOFOCUS_GAIN:
                        //判断是否需要重新播放音乐

                        break;
                    //暂时失去焦点
                    case AudioManager.AUDIOFOCUS_LOSS_TRANSIENT:
                        //暂时失去焦点，暂停播放音乐（将needRestart设置为true）

                        break;
                    //时期焦点
                    case AudioManager.AUDIOFOCUS_LOSS:
                        //暂停播放音乐，不再继续播放

                        break;
                }
            }
        };
        AudioManager manager = (AudioManager) mContext.getSystemService(Context.AUDIO_SERVICE);
        manager.requestAudioFocus(afChangeListener,AudioManager.STREAM_MUSIC,AudioManager.AUDIOFOCUS_GAIN_TRANSIENT);
    }

    /**
     * 判断屏幕旋转功能是否开启
     */
    public static boolean isOpenSensor(){
        boolean isOpen = false;
        if(getSensorState(mContext) == 1){
            isOpen = true;
        }else if(getSensorState(mContext) == 0){
            isOpen = false;
        }
        return isOpen;
    }

    public static void pickImage(String source, boolean cropped, int maxSize){
        Intent intent = new Intent(mContext, PickImageActivity.class);
        intent.putExtra("source", source);
        intent.putExtra("cropped", cropped);
        intent.putExtra("maxSize", maxSize);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        mContext.startActivity(intent);
    }

    public static void pickVideo(String source){
        Intent intent = new Intent(Intent.ACTION_PICK, android.provider.MediaStore.Video.Media.EXTERNAL_CONTENT_URI);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        mContext.startActivity(intent);
    }

    private static int getSensorState(Context context){
        int sensorState = 0;
        try {
            sensorState = Settings.System.getInt(context.getContentResolver(), Settings.System.ACCELEROMETER_ROTATION);
            return sensorState;
        } catch (Settings.SettingNotFoundException e) {
            e.printStackTrace();
        }
        return sensorState;
    }

    public static String getDeviceID() {
        String uuid = UUIDUtils.getUUID();
        return uuid;
    }
    
    public static boolean isEnableNotification(){
        NotificationManagerCompat notification = NotificationManagerCompat.from(mContext);
        boolean isEnabled = notification.areNotificationsEnabled();
        return isEnabled;
    }
}
