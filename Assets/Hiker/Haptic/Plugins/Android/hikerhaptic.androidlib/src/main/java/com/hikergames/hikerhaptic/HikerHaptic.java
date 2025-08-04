package com.hikergames.hikerhaptic;

import android.app.Activity;
import android.content.Context;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.util.Log;

public class HikerHaptic
{
    Context _context;
    Activity _activity;
    Vibrator _vibrator;

    // static final VibrationEffect TAP_EFFECT;

    public HikerHaptic(Activity activity)
    {
        _activity = activity;
        _context = activity.getBaseContext();

        _vibrator = (Vibrator) _context.getSystemService(Context.VIBRATOR_SERVICE);

        // if (Build.VERSION.SDK_INT >= 26)
        // {
        //     TAP_EFFECT = VibrationEffect.createOneShot(1, 200);
        // }

        if (_activity == null) {
            Log.w("HikerHaptic", "Activity is null");
        }
        if (_context == null) {
            Log.w("HikerHaptic", "Context is null");
        }
        if (_vibrator == null) {
            Log.w("HikerHaptic", "Vibrator was not initialized");
        }
        else
        {
            if (_vibrator.hasVibrator() == false)
            {
                Log.d("HikerHaptic", "The hardware does not have a vibrator");
            }
            
            if (Build.VERSION.SDK_INT < 26)
            {
                Log.d("HikerHaptic", "The Android API is lower than 26 so does not support Amplitude control");
            }
            else if (_vibrator.hasAmplitudeControl () == false)
            {
                Log.d("HikerHaptic", "The hardware does not have Amplitude control");
            }

            if (Build.VERSION.SDK_INT >= 30)
            {
                var supp = _vibrator.areEffectsSupported( new int[] { VibrationEffect.EFFECT_CLICK, VibrationEffect.EFFECT_DOUBLE_CLICK,
                 VibrationEffect.EFFECT_HEAVY_CLICK, VibrationEffect.EFFECT_TICK});
                
                Log.d("HikerHaptic", String.format("EFFECT_CLICK status support is %d", supp[0]));
                Log.d("HikerHaptic", String.format("EFFECT_DOUBLE_CLICK status support is %d", supp[1]));
                Log.d("HikerHaptic", String.format("EFFECT_HEAVY_CLICK status support is %d", supp[2]));
                Log.d("HikerHaptic", String.format("EFFECT_TICK status support is %d", supp[3]));
            }
        }
    }

    public boolean isSupportHaptic()
    {
        if (Build.VERSION.SDK_INT < 26)
        {
            return false;
        }

        if (_vibrator == null || _vibrator.hasVibrator() == false || _vibrator.hasAmplitudeControl() == false)
        {
            return false;
        }

        return true;
    }

    public void playOneShot(int millisec, int amplitude)
    {
        if (millisec <= 0 || amplitude <= 0) return;

        _vibrator.cancel();
        if (Build.VERSION.SDK_INT >= 26) {
            _vibrator.vibrate(VibrationEffect.createOneShot(millisec, amplitude));
        }
        else
        {
            Log.w("HikerHaptic", "Vibrator API < 26");
            _vibrator.vibrate(millisec);
        }
    }

    public void playPredefined(int effectId)
    {
        if (effectId < 0) return;

        _vibrator.cancel();
        if (Build.VERSION.SDK_INT >= 26) {
            _vibrator.vibrate(VibrationEffect.createPredefined(effectId));
        }
        else
        {
            Log.w("HikerHaptic", "Vibrator API < 26");
        }
    }

    public void playTapEffect()
    {
        _vibrator.cancel();
        if (Build.VERSION.SDK_INT >= 26) {
            VibrationEffect TAP_EFFECT = VibrationEffect.createOneShot(1, 200);
            _vibrator.vibrate(TAP_EFFECT);
        }
        else
        {
            Log.w("HikerHaptic", "Vibrator API < 26");
        }
    }

    public void vibrateLight()
    {

//    val vibrator = context?.getSystemService(Context.VIBRATOR_SERVICE) as Vibrator
        if (Build.VERSION.SDK_INT >= 26) {
            _vibrator.vibrate(VibrationEffect.createOneShot(200, VibrationEffect.DEFAULT_AMPLITUDE));
        }
        else
        {
            _vibrator.vibrate(200);
        }
    }
}
