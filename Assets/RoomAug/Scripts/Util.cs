using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;


public static class Util
{
	//Used by gameplayRooms, and PandaCubes.
	public enum ElementalType {
		None,
		Wood,
		Fire,
		Earth,
		Metal,
		Water
	}

	public static Color GetColor( ElementalType elementalType ) {
		switch (elementalType)
		{
			case ElementalType.None:
				return Color.white;
				break;
			case ElementalType.Wood:
				return Color.green;
				break;
			case ElementalType.Fire:
				return Color.red;
				break;
			case ElementalType.Earth:
				return Color.yellow;
				break;
			case ElementalType.Metal:
				return Color.grey;
				break;
			case ElementalType.Water:
				return Color.blue;
				break;
			default:
				Debug.LogError ( "Invalid CubeType" );
				return Color.black;
		}
	}


    public static List<T> CreateList<T> ( params T [] values )
    {
        return new List<T> ( values );
    }

    public static BaseGameplayObject[] getGameplayObjects ( bool inclDisabled = false )
    {
        return ( inclDisabled ? Resources.FindObjectsOfTypeAll ( typeof ( BaseGameplayObject ) ) as BaseGameplayObject []
			: UnityEngine.Object.FindObjectsOfType ( typeof ( BaseGameplayObject ) ) as BaseGameplayObject [] );
    }

    public static void collectGameplayObjects ( ref HashSet<BaseGameplayObject> m_gameplayObjects )
    {
        collectHashSetOfComponents<BaseGameplayObject> ( ref m_gameplayObjects );
    }

    public static void collectHashSetOfComponents<T> ( ref HashSet<T> setToFill, bool inclDisabled = false )
    {
        if (setToFill == null)
            setToFill = new HashSet<T> ();
        else
            setToFill.Clear ();

        T [] prs = inclDisabled ? Resources.FindObjectsOfTypeAll ( typeof ( T ) ) as T [] : UnityEngine.Object.FindObjectsOfType ( typeof ( T ) ) as T [];

        foreach ( T pr in prs )
        {
            setToFill.Add ( pr );
        }
    }

    //LOGGERS

    //Strings
    public static void JLog ( string val, bool toast = false )
    {
        Debug.Log ( "J# " + val );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( val );

    }

    public static void JLogErr ( string val, bool toast = false )
    {
        Debug.LogError ( "J# " + val );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( val );
    }

    //Ints
    public static void JLog ( int val, bool toast = false )
    {
        Debug.Log ( "J# " + val );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( val.ToString () );

    }

    public static void JLogErr ( int val, bool toast = false )
    {
        Debug.LogError ( "J# " + val );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( val.ToString () );
    }

    //Arrays
    public static void JLog<T> ( T [] arr, bool toast = false )
    {

        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ( "J# [" );
        foreach ( T t in arr )
        {
            builder.AppendLine ( " " + t.ToString () );
        }
        builder.AppendLine ( "J# [" );

        Debug.Log ( builder.ToString () );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( builder.ToString () );

    }

    public static void JLogErr<T> ( T [] arr, bool toast = false )
    {
        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ( "J# [" );
        foreach ( T t in arr )
        {
            builder.AppendLine ( " " + t.ToString () );
        }
        builder.AppendLine ( "J# [" );

        Debug.LogError ( builder.ToString () );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( builder.ToString () );
    }


    //Generic Lambda
    public static void JLog<T> ( T val, Func<T, string> selector, bool toast = false )
    {
        Debug.Log ( "J# " + selector ( val ) );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( selector ( val ) );
    }

    public static void JLogErr<T> ( T val, Func<T, string> selector, bool toast = false )
    {
        Debug.LogError ( "J# " + selector ( val ) );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( selector ( val ) );
    }



    //Generic Lambda Arrays
    //eg   Util.JLogArr(Application.GetBuildTags(), x => x.ToString() , true);
    public static void JLogArr<T> ( T [] arr, Func<T, string> selector, bool toast = false )
    {
        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ( "J# [" );
        foreach ( T t in arr )
        {
            builder.AppendLine ( " " + selector ( t ) );
        }
        builder.AppendLine ( "]" );

        Debug.Log ( builder.ToString () );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( builder.ToString () );
    }

    //Generic Lambda Arrays
    public static void JLogArrErr<T> ( T [] arr, Func<T, string> selector, bool toast = false )
    {
        StringBuilder builder = new StringBuilder ();
        builder.AppendLine ( "J# [" );
        foreach ( T t in arr )
        {
            builder.Append ( "  " + selector ( t ) );
        }
        builder.Append ( "]" );

        Debug.LogError ( builder.ToString () );
        if (toast)
            AndroidHelper.ShowAndroidToastMessage ( builder.ToString () );
    }
}