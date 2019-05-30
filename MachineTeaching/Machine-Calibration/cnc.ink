schema CNCState
    Float32 error,
    Float32 time
end

schema CNCAction
    Float32{0:1} calibration_adjustment
end

schema CNCConfig
    Float32 motor_alignment,
    Float32 bit_alignment,
    Float32 slider_alignment
end

concept machine_calibrator is estimator
   predicts (CNCAction)
   follows input(CNCState)
   feeds output
end

simulator cnc_simulator(CNCConfig)
    action (CNCAction)
    state (CNCState)
end

curriculum my_curriculum
    train machine_calibrator
    using algorithm TRPO
    with simulator cnc_simulator
    objective speed_and_accuracy
        lesson my_first_lesson
            configure
               constrain motor_alignment with Float32{5.0:20.0},
               constrain bit_alignment with Float32{5.0:20.0},
               constrain slider_alignment with Float32{5.0:20.0}
            until
                maximize speed_and_accuracy
end