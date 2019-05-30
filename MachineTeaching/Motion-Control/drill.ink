schema DrillState
    Float32 side_force,
    Float32 inclination
end
 
schema DrillAction
    Float32{0:1} drill_command
end
 
schema DrillConfig
    Float32 starting_point,
    Float32 starting_angle
end
 
concept horizontal_drill is estimator
   predicts (DrillAction)
   follows input(DrillState)
   feeds output
end
 
simulator drill_simulator(DrillConfig)
    action (DrillAction)
    state (DrillState)
end
 
curriculum my_curriculum
    train horizontal_drill
    using algorithm TRPO
    with simulator drill_simulator
    objective speed_and_accuracy
        lesson my_first_lesson
            configure
               constrain starting_point with Float32{5.0:20.0},
               constrain starting_angle with Float32{-3.14:3.14}
            until
                maximize speed_and_accuracy
end
 