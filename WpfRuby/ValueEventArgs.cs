namespace SorcererSoftware {
   /// <summary>
   /// This really should be part of the CLR
   /// </summary>
   public class ValueEventArgs<T> : System.EventArgs {
      public readonly T Value;
      public ValueEventArgs(T value) { Value = value; }
      public static implicit operator ValueEventArgs<T>(T value) { return new ValueEventArgs<T>(value); }
   }
}
