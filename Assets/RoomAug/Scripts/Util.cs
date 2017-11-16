using UnityEngine;
using System.Linq;
using System.Text;
using System;


public static class Util  {

    //Strings
	public static void JLog( string val, bool toast = false) {
		Debug.Log ("J# " + val);
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( val );

	}
	public static void JLogErr( string val, bool toast = false) {
		Debug.LogError ("J# " + val);
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( val );
	}

    //Ints
	public static void JLog( int val, bool toast = false) {
		Debug.Log ("J# " + val);
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( val.ToString() );

	}
	public static void JLogErr( int val, bool toast = false) {
        Debug.LogError ("J# " + val );
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( val.ToString() );
	}

    //Arrays
    public static void JLog<T>( T[] arr, bool toast = false ) {

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("J# [");
        foreach( T t in arr ) {
            builder.AppendLine( " " + t.ToString() );
        }
        builder.AppendLine( "J# [" );

        Debug.Log( builder.ToString() );
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( builder.ToString() );

    }
    public static void JLogErr<T>( T[] arr, bool toast = false ) {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine( "J# [" );
        foreach ( T t in arr ) {
            builder.AppendLine( " " + t.ToString() );
        }
        builder.AppendLine( "J# [" );

        Debug.LogError( builder.ToString() );
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( builder.ToString() );
    }


    //Generic Lambda
    public static void JLog<T>( T val, Func<T, string> selector, bool toast = false ) {
        Debug.Log( "J# " + selector( val ) );
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( selector( val ) );
    }
    public static void JLogErr<T>( T val, Func<T, string> selector, bool toast = false ) {
        Debug.LogError( "J# " + selector( val ) );
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( selector( val ) );
    }



    //Generic Lambda Arrays
    //eg   Util.JLogArr(Application.GetBuildTags(), x => x.ToString() , true);
    public static void JLogArr<T>( T[] arr, Func<T, string> selector, bool toast = false ) {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine( "J# [" );
        foreach ( T t in arr ) {
            builder.AppendLine( " " + selector(t) );
        }
        builder.AppendLine( "]" );

        Debug.Log( builder.ToString() );
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( builder.ToString() );
    }

    //Generic Lambda Arrays
    public static void JLogArrErr<T>( T[] arr, Func<T, string> selector, bool toast = false ) {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine( "J# [" );
        foreach ( T t in arr ) {
            builder.Append( "  " + selector( t ) );
        }
        builder.Append( "]" );

        Debug.LogError( builder.ToString() );
        if ( toast ) AndroidHelper.ShowAndroidToastMessage( builder.ToString() );
    }
}