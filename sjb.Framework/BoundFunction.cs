using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;

namespace sjb.Framework
{
    /// <summary>
    /// BoundFunction<TSource, T0, T1, ..., Tn, TOut> provides an easy way to calculate a result 
    /// from inputs, where the inputs are bound property paths and the result implements INotifyPropertyChanged. 
    /// 
    /// For example, say you have a ViewModel whose type is TSource containing properties Cmd and Service, and suppose
    /// Cmd has a bool property IsExecuting while Service has a string property Name. Say you want to bind a property 
    /// in your View to the propertyPath "(Service.Name == "Flooble")? Cmd.IsComplete: true", representing the execution status.
    /// 
    /// Then you declare in your ViewModel
    ///     public BoundFunction<bool> ExecutionStatus;
    /// and you initialize it in your constructor like so: 
    ///     this.ExecutionStatus = BoundFunction.Create( 
    ///         this,
    ///         (name, isComplete) => ((name == "Flooble")? isComplete: true), 
    ///         (vm) => vm.Service.Name
    ///         (vm) => vm.Cmd.IsComplete
    ///     ); 
    /// and then you can use {x:Bind ExecutionStatus.Value, Mode=OneWay} in your View XAML. The ExecutionStatus.Value will 
    /// update whenever either one of the inputs (this.Service.Name or this.Cmd.IsComplete) updates. 
    /// 
    /// As in all binding scenarios, the properties (Cmd and Service, in this example) should be notified with
    /// PropertyChanged. This is made easier if they are just assigned in the constructor and never changed; in that
    /// case obviously you don't have to issue the PropertyChanged. So basically instead of declaring
    ///     public readonly Command Cmd;
    /// you instead declare
    ///     public Command Cmd { get; private set; }
    /// then initialize it in the constructor and never change it. Assuming you are not going to notify these
    /// input properties then they have to be initialized before the BoundFunction is created. 
    /// 
    /// The type of BoundFunction returned by Create, in this example, is BoundFunction<TSource,string,bool,bool>.
    /// The lambda propertyPath (name, isComplete) ==> (...) has type Func<string,bool,bool>, which is the type of the
    /// function being bound. The two remaining arguments have type Func<TSource,string> and Func<TSource,bool> 
    /// representing (in a type-safe manner) the binding paths for the two inputs. For example, (vm) => vm.Cmd.IsComplete
    /// refers to the fact that starting from the binding source, which is a view model (vm) of type TSource, the 
    /// binding path "Cmd.IsComplete" will be applied.
    ///  
    /// The use of BoundFunction<bool> to declare ExecutionStatus is a convenience; BoundFunction<TOut> is 
    /// the base class of BoundFunction<TSource,string,bool,bool>. Only the output type of the propertyPath is specified
    /// in the base class. You could also use public readonly var ExecutionStatus = BoundFunction.Create( ...), and if 
    /// you did that then ExecutionStatus would have the derived type BoundFunction<TSource,string,bool,bool>.
    /// 
    /// This example uses the Create (factory) method, but you could also just use the constructor. If you do use 
    /// the constructor then you must also specify the type arguments after the type name, since the compiler will 
    /// not infer the type of object to be constructed based on the types of the constructor arguments.  
    /// Notice that the types for name and isComplete in the lambda propertyPath are inferred from the types of the
    /// last two arguments specified to the Create method. 
    /// 
    /// In the example, "this" refers to this ViewModel, of type TSource. It will be used as the Source of a Binding 
    /// whose Path is "Service.Name" and also as the Source of another Binding whose Path is "Cmd.IsComplete". 
    /// When either of these Bindings update, the ExecutionStatus.Value is recalculated and PropertyChanged occurs
    /// with property name = "Value". The value set is calculated using the function 
    ///     (name, isComplete) => ((name == "Flooble")? isComplete: true)
    /// using the two values obtained from the two bindings. 
    /// 
    /// Note the BindingFunctiontion.Value can only be used in a one-way or one-time binding; not in a two-way binding. 
    /// 
    /// The FallbackValue for the first binding in the example is null and for the second it is false; 
    /// in general it is default(Ti) for the ith input type. However you can also hand in these fallback 
    /// values as the final arguments of the constructor. 
    /// </summary>
    /// <typeparam name="TSource">Type of the object relative to which the propertyPaths are specified.</typeparam>
    /// <typeparam name="Ti">Type of the (i+1)st input to the function</typeparam>
    /// <typeparam name="TOut">Type of the function result</typeparam>
    /// 
    public class BoundFunction
    {
        public static BoundFunction<TSource, T0, TOut> Create<TSource, T0, TOut>(
            TSource source,
            Func<T0, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            T0 fallbackValue0 = default(T0)
        )
        {
            return new BoundFunction<TSource, T0, TOut>(
                source,
                function,
                propertyPath0,
                fallbackValue0
            );
        }


        public static BoundFunction<TSource, T0, T1, TOut> Create<TSource, T0, T1, TOut>(
            TSource source,
            Func<T0, T1, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1)
        )
        {
            return new BoundFunction<TSource, T0, T1, TOut>(
                source,
                function,
                propertyPath0,
                propertyPath1,
                fallbackValue0,
                fallbackValue1
            );
        }


        public static BoundFunction<TSource, T0, T1, T2, TOut> Create<TSource, T0, T1, T2, TOut>(
            TSource source,
            Func<T0, T1, T2, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2)
        )
        {
            return new BoundFunction<TSource, T0, T1, T2, TOut>(
                source,
                function,
                propertyPath0,
                propertyPath1,
                propertyPath2,
                fallbackValue0,
                fallbackValue1,
                fallbackValue2
            );
        }


        public static BoundFunction<TSource, T0, T1, T2, T3, TOut> Create<TSource, T0, T1, T2, T3, TOut>(
            TSource source,
            Func<T0, T1, T2, T3, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3)
        )
        {
            return new BoundFunction<TSource, T0, T1, T2, T3, TOut>(
                source,
                function,
                propertyPath0,
                propertyPath1,
                propertyPath2,
                propertyPath3,
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3
            );
        }


        public static BoundFunction<TSource, T0, T1, T2, T3, T4, TOut> Create<TSource, T0, T1, T2, T3, T4, TOut>(
            TSource source,
            Func<T0, T1, T2, T3, T4, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            Expression<Func<TSource, T4>> propertyPath4,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3),
            T4 fallbackValue4 = default(T4)
        )
        {
            return new BoundFunction<TSource, T0, T1, T2, T3, T4, TOut>(
                source,
                function,
                propertyPath0,
                propertyPath1,
                propertyPath2,
                propertyPath3,
                propertyPath4,
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3,
                fallbackValue4
            );
        }


        public static BoundFunction<TSource, T0, T1, T2, T3, T4, T5, TOut> Create<TSource, T0, T1, T2, T3, T4, T5, TOut>(
            TSource source,
            Func<T0, T1, T2, T3, T4, T5, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            Expression<Func<TSource, T4>> propertyPath4,
            Expression<Func<TSource, T5>> propertyPath5,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3),
            T4 fallbackValue4 = default(T4),
            T5 fallbackValue5 = default(T5)
        )
        {
            return new BoundFunction<TSource, T0, T1, T2, T3, T4, T5, TOut>(
                source,
                function,
                propertyPath0,
                propertyPath1,
                propertyPath2,
                propertyPath3,
                propertyPath4,
                propertyPath5,
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3,
                fallbackValue4,
                fallbackValue5
            );
        }


    }


    public abstract class BoundFunction<TOut> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public TOut Value
        {
            get
            {
                return _value;
            }
            protected set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
            }
        }
        private TOut _value;

        private readonly Binding[] Bindings;
        private readonly InputReceptor[] InputReceptors;
        private readonly bool UnderConstruction;

        protected object Input(uint i)
        {
            return InputReceptors[i].Value;
        }

        protected abstract TOut CalculateValue(); // implement this by applying the Function to the InputReceptors. 

        protected BoundFunction(object source, string[] paths, object[] fallbackValues)
        {
            UnderConstruction = true;
            Bindings = new Binding[paths.Length];
            InputReceptors = new InputReceptor[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                Bindings[i] = new Binding() { Source = source, Path = new PropertyPath(paths[i]), Mode = BindingMode.OneWay, FallbackValue = fallbackValues[i] };
                InputReceptors[i] = new InputReceptor(this);
                BindingOperations.SetBinding(InputReceptors[i], InputReceptor.ValueProperty, Bindings[i]);
            }
            UnderConstruction = false;
        }

        protected void OnInputChanged()
        {
            if (!UnderConstruction)
            { // SetBinding causes an immediate update. We want to skip it until after the derived class Function is assigned.
                Value = CalculateValue();
            }
        }

        private class InputReceptor : DependencyObject
        {
            public InputReceptor(BoundFunction<TOut> boundFunction)
            {
                BoundFunction = boundFunction;
                //this.RegisterPropertyChangedCallback(InputReceptor.ValueProperty, this.ValuePropertyChanged);
            }

            private BoundFunction<TOut> BoundFunction { get; set; }

            public object Value
            {
                get { return GetValue(ValueProperty); }
                private set { SetValue(ValueProperty, value); }
            }
            public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(InputReceptor), new PropertyMetadata(null));

            private void ValuePropertyChanged() //DependencyObject sender, DependencyProperty dp)
            {
                BoundFunction.OnInputChanged();
            }
        }

    }


    public sealed class BoundFunction<TSource, T0, TOut> : BoundFunction<TOut>
    {
        private readonly Func<T0, TOut> Function;

        public BoundFunction(
            TSource source,
            Func<T0, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            T0 fallbackValue0 = default(T0)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0)
            },
            new object[] {
                fallbackValue0
            }
        )
        {
            Function = function;
            base.OnInputChanged(); // initialize base.Value using this.Function
        }

        protected override TOut CalculateValue()
        {
            return Function((T0)base.Input(0));
        }
    }


    public sealed class BoundFunction<TSource, T0, T1, TOut> : BoundFunction<TOut>
    {
        private readonly Func<T0, T1, TOut> Function;

        public BoundFunction(
            TSource source,
            Func<T0, T1, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1)
            }, new object[] {
                fallbackValue0,
                fallbackValue1
            }
        )
        {
            Function = function;
            base.OnInputChanged(); // initialize base.Value using this.Function
        }

        protected override TOut CalculateValue()
        {
            return Function(
                (T0)base.Input(0),
                (T1)base.Input(1)
            );
        }
    }


    public sealed class BoundFunction<TSource, T0, T1, T2, TOut> : BoundFunction<TOut>
    {
        private readonly Func<T0, T1, T2, TOut> Function;

        public BoundFunction(
            TSource source,
            Func<T0, T1, T2, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1),
                Property.Path(propertyPath2)
            }, new object[] {
                fallbackValue0,
                fallbackValue1,
                fallbackValue2
            }
        )
        {
            Function = function;
            base.OnInputChanged(); // initialize base.Value using this.Function
        }

        protected override TOut CalculateValue()
        {
            return Function(
                (T0)base.Input(0),
                (T1)base.Input(1),
                (T2)base.Input(2)
            );
        }
    }


    public sealed class BoundFunction<TSource, T0, T1, T2, T3, TOut> : BoundFunction<TOut>
    {
        private readonly Func<T0, T1, T2, T3, TOut> Function;

        public BoundFunction(
            TSource source,
            Func<T0, T1, T2, T3, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1),
                Property.Path(propertyPath2),
                Property.Path(propertyPath3)
            }, new object[] {
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3
            }
        )
        {
            Function = function;
            base.OnInputChanged(); // initialize base.Value using this.Function
        }

        protected override TOut CalculateValue()
        {
            return Function(
                (T0)base.Input(0),
                (T1)base.Input(1),
                (T2)base.Input(2),
                (T3)base.Input(3)
            );
        }
    }


    public sealed class BoundFunction<TSource, T0, T1, T2, T3, T4, TOut> : BoundFunction<TOut>
    {
        private readonly Func<T0, T1, T2, T3, T4, TOut> Function;

        public BoundFunction(
            TSource source,
            Func<T0, T1, T2, T3, T4, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            Expression<Func<TSource, T4>> propertyPath4,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3),
            T4 fallbackValue4 = default(T4)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1),
                Property.Path(propertyPath2),
                Property.Path(propertyPath3),
                Property.Path(propertyPath4)
            }, new object[] {
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3,
                fallbackValue4
            }
        )
        {
            Function = function;
            base.OnInputChanged(); // initialize base.Value using this.Function
        }

        protected override TOut CalculateValue()
        {
            return Function(
                (T0)base.Input(0),
                (T1)base.Input(1),
                (T2)base.Input(2),
                (T3)base.Input(3),
                (T4)base.Input(4)
            );
        }
    }


    public sealed class BoundFunction<TSource, T0, T1, T2, T3, T4, T5, TOut> : BoundFunction<TOut>
    {
        private readonly Func<T0, T1, T2, T3, T4, T5, TOut> Function;

        public BoundFunction(
            TSource source,
            Func<T0, T1, T2, T3, T4, T5, TOut> function,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            Expression<Func<TSource, T4>> propertyPath4,
            Expression<Func<TSource, T5>> propertyPath5,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3),
            T4 fallbackValue4 = default(T4),
            T5 fallbackValue5 = default(T5)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1),
                Property.Path(propertyPath2),
                Property.Path(propertyPath3),
                Property.Path(propertyPath4),
                Property.Path(propertyPath5)
            }, new object[] {
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3,
                fallbackValue4,
                fallbackValue5
            }
        )
        {
            Function = function;
            base.OnInputChanged(); // initialize base.Value using this.Function
        }

        protected override TOut CalculateValue()
        {
            return Function(
                (T0)base.Input(0),
                (T1)base.Input(1),
                (T2)base.Input(2),
                (T3)base.Input(3),
                (T4)base.Input(4),
                (T5)base.Input(5)
            );
        }
    }


}
