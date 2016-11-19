using UnityEngine;
using System.Collections;

public class EmptyFieldValidator : FieldValidator {

    public override bool validate(Field f)
    {
        return f.isEmpty();
    }
}
