function Get-HashTableFromProcessedBehaviorArray($BehaviorArray) {
    $behaviorHash = @{}
    foreach($behavior in $BehaviorArray) {
        $behaviorHash[$behavior[0]] = $behavior
    }
    return $behaviorHash
}