# Transform an array of config behaviors to a hash table where the key is the path
# specified in the behavior and the value is the behavior object.
# Example:
# Assets:
#   - Path: "/tenancy"
#   - Path: "/yada"
#     Guid: "1234"
# becomes
#  behaviors["/tenancy"] = @{ Path = "/tenancy" }
#  becomes["/yada"] = @{ Path = "/yada", Guid = "1234" }
function Get-HashTableFromBehaviorArray($BehaviorArray) {
    $behaviorHash = @{}
    foreach($behavior in $BehaviorArray) {
        $behaviorHash[$behavior.Path] = $behavior
    }
    return $behaviorHash
}