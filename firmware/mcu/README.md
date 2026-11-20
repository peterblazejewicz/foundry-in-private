# firmware/mcu/

The STM32H5 sketch (Zephyr, Arduino RPC bridge) for the reject gate.

- It enforces the servo range, duty limits and the e-stop, whatever board B asks (architecture §5.2, §6.3).
- It answers each `servo.move` with the §7.5 acknowledgement.
- The PWM pin is to be confirmed in the user manual (architecture §4.4).
