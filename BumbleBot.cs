using System;
using System.Drawing;
using Robocode;

namespace CAP4053.Student
{
    public class BumbleBot : TeamRobot
    {
        // Abstract class representing the states of the robot
        public abstract class State
        {
            public abstract void ExecuteState(BumbleBot robot);
        }

        // State to scan for a single enemy
        public class ScanEnemyState : State
        {
            // If multiple enemies detected, switch to ScanEnemiesState
            public override void ExecuteState(BumbleBot robot)
            {
                if (robot.MultipleEnemies())
                {
                    robot.currentState = new ScanEnemiesState();
                }
                // Move radar to scan for a single enemy
                robot.MoveRadarOneEnemy();
                // Check if close to wall and switch state if necessary
                if (robot.IsCloseToWall())
                {
                    robot.currentState = new MoveTankWallState();
                }
                // Check if enemy is too close and switch state if necessary
                else if (robot.IsEnemyClose())
                {
                    robot.currentState = new MoveTankAwayState();
                }
                // Check if robot is unhealthy and switch state if necessary
                else if (robot.IsUnhealthy())
                {
                    robot.currentState = new MoveTankRandState();
                }
                // Move tank by default
                robot.currentState = new MoveTankState();
            }
        }

        // State to scan for multiple enemies
        public class ScanEnemiesState : State
        {
            public override void ExecuteState(BumbleBot robot)
            {
                // Move radar to scan for multiple enemies
                robot.MoveRadar();
                // Check if close to wall and switch state if necessary
                if (robot.IsCloseToWall())
                {
                    robot.currentState = new MoveTankWallState();
                }
                // Check if enemy is too close and switch state if necessary
                else if (robot.IsEnemyClose())
                {
                    robot.currentState = new MoveTankAwayState();
                }
                // Check if robot is unhealthy and switch state if necessary
                else if (robot.IsUnhealthy())
                {
                    robot.currentState = new MoveTankRandState();
                }
                // Move tank by default
                robot.currentState = new MoveTankState();
            }
        }

        // State to move the tank
        public class MoveTankState : State
        {
            public override void ExecuteState(BumbleBot robot)
            {
                // Move the tank
                robot.MoveTank();
                // Check if robot is unhealthy and switch state if necessary
                if (robot.IsUnhealthy())
                {
                    robot.currentState = new FireGunEnergyBasedState();
                }
                // Move and fire gun by default
                robot.currentState = new MoveAndFireGunState();
            }
        }

        // State to move the tank when close to a wall
        public class MoveTankWallState : State
        {
            public override void ExecuteState(BumbleBot robot)
            {
                // Move the tank near the wall
                robot.MoveTankWall();
                // Check if robot is unhealthy and switch state if necessary
                if (robot.IsUnhealthy())
                {
                    robot.currentState = new FireGunEnergyBasedState();
                }
                // Move and fire gun by default
                robot.currentState = new MoveAndFireGunState();
            }
        }

        // State to move the tank away from the enemy
        public class MoveTankAwayState : State
        {
            public override void ExecuteState(BumbleBot robot)
            {
                // Move the tank away from the enemy
                robot.MoveTankAway();
                // Check if robot is unhealthy and switch state if necessary
                if (robot.IsUnhealthy())
                {
                    robot.currentState = new FireGunEnergyBasedState();
                }
                // Move and fire gun by default
                robot.currentState = new MoveAndFireGunState();
            }
        }

        // State to move the tank randomly
        public class MoveTankRandState : State
        {
            public override void ExecuteState(BumbleBot robot)
            {
                // Move the tank randomly
                robot.MoveTankRand();
                // Check if robot is unhealthy and switch state if necessary
                if (robot.IsUnhealthy())
                {
                    robot.currentState = new FireGunEnergyBasedState();
                }
                // Move and fire gun by default
                robot.currentState = new MoveAndFireGunState();
            }
        }

        // State to move and fire the gun
        public class MoveAndFireGunState : State
        {
            public override void ExecuteState(BumbleBot robot)
            {
                // Move and fire the gun
                robot.MoveAndFireGun();
                // If multiple enemies detected, switch to ScanEnemiesState
                if (robot.MultipleEnemies())
                {
                    robot.currentState = new ScanEnemiesState();
                }
                // Otherwise, switch to ScanEnemyState
                robot.currentState = new ScanEnemyState();
            }
        }
        
        // State to fire the gun based on energy level
        public class FireGunEnergyBasedState : State
        {
            public override void ExecuteState(BumbleBot robot)
            {
                // Move and fire the gun based on energy level
                robot.MoveAndFireGunEnergy();
                // If multiple enemies detected, switch to ScanEnemiesState
                if (robot.MultipleEnemies())
                {
                    robot.currentState = new ScanEnemiesState();
                }
                // Otherwise, switch to ScanEnemyState
                robot.currentState = new ScanEnemyState();
            }
        }

        // Constructor initializes the robot's current state
        public BumbleBot()
        {
            currentState = new ScanEnemiesState();
        }

        private EnemyTank enemy = new EnemyTank();

        // Robot attributes and properties, avoiding magic numbers
        private int enemiesCount = 0;
        private int direction = 1;
        private const int maxVelocity = 8;
        private const int scanAngleMult = 360;
        private const int scanAngleOne = 180;
        private const int genAngle = 85;
        private const int strafe = 90;
        private const int genFirePow = 475;

        private State currentState;

        // My robot behavior loop
        public override void Run()
        {
            InitializeRobotSettings();

            while (true)
            {
                currentState.ExecuteState(this);
                Execute();
            }
        }

        // Intiailize robot's independent radar and gun movement and colors
        private void InitializeRobotSettings()
        {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;
            BodyColor = Color.Black;
            GunColor = Color.Yellow;
            ScanColor = Color.Black;
            RadarColor = Color.Black;
            BulletColor = Color.Yellow;
        }

        // Event handler for scanning an enemy or team bot
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            // Update enemy information if necessary
            if (ShouldUpdate(e))
            {
                enemy.Update(e, this);
                enemiesCount++;
            }
        }

        private bool ShouldUpdate(ScannedRobotEvent e)
        {
            // If the scanned robot is a teammate, ignore
            if (IsTeammate(e.Name))
            {
                return false;
            }
            // If no enemy exists, update
            else if (enemy.NoEnemy())
            {
                return true;
            }
            // If scanned robot is closer or if enemy is the same enemy, return true
            return e.Name == enemy.GetName() || e.Distance < enemy.GetDistance();
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            // If the dead robot was the current enemy, reset enemy information and decrease enemies count
            if (e.Name == enemy.GetName())
            {
                enemy.NewBot();
                enemiesCount--;
            }
        }

        // Move radar to scan for multiple enemies
        public void MoveRadar()
        {
            SetTurnRadarRight(scanAngleMult);
        }

        // Move radar to scan for single enemy
        public void MoveRadarOneEnemy()
        {
            SetTurnRadarRight(scanAngleOne);
        }

        // Randomly moves the tank by generating a random angle within a range and adjusting the tank's movement direction accordingly.
        
        // Inspired by STRAFING implementation from: https://mark.random-article.com/robocode/basic_movement.html
        public void MoveTankRand()
        {
            Random random = new Random();
            // Generate a random angle within the range of 0 to 20 degrees
            double randomAngle = random.NextDouble() * 20;
            // Calculate the turn angle by adding the enemy's bearing angle, a general angle, and the random angle
            double turnAngle = NormalizeAngle(enemy.GetBearing() + genAngle - randomAngle * direction);
            SetTurnRight(turnAngle);
            if (Velocity == 0)
            {
                // strafing implementation
                direction *= -1;
                SetAhead(strafe * direction);
                MaxVelocity = maxVelocity;
            }
        }

        public void MoveTank()
        {
            // Calculate the turn angle by adding the enemy's bearing angle and a general angle, adjusting for direction
            double turnAngle = NormalizeAngle(enemy.GetBearing() + genAngle - 20 * direction);
            SetTurnRight(turnAngle);
            if (Velocity == 0)
            {
                direction *= -1;
                SetAhead(strafe * direction);
                MaxVelocity = maxVelocity;
            }
        }

        public void MoveTankAway()
        {
            // Turn around by adding 180 degrees to the enemy's bearing angle
            double turnAngle = NormalizeAngle(enemy.GetBearing() + 180);
            SetTurnRight(turnAngle);
            // Move away from the enemy
            SetAhead(strafe);
            MaxVelocity = maxVelocity;
        }

        public void MoveTankWall()
        {
            // Calculate the turn angle by adding the enemy's bearing angle, a general angle, and adjusting for direction
            double turnAngle = NormalizeAngle(enemy.GetBearing() + genAngle - 20 * direction);
            SetTurnRight(turnAngle);
            // Define a margin from the wall
            int wallMargin = 60;
            double distanceToWall = Math.Min(Math.Min(X, BattleFieldWidth - X), Math.Min(Y, BattleFieldHeight - Y));
            // Calculate the movement distance considering the wall margin
            double movementDistance = Math.Max(50, distanceToWall - wallMargin);

            if (Velocity == 0)
            {
                direction *= -1;
                SetAhead(movementDistance * direction);
                MaxVelocity = maxVelocity;
            }
        }

        // PREDICTIVE AIMING for both gun functions inspired from: https://mark.random-article.com/robocode/improved_targeting.html

        public void MoveAndFireGun()
        {
            // Increased overall firewpower due to my tank's good health
            // Calculate the base firepower based on the distance to the enemy
            double baseFirePow = Math.Min(genFirePow + 100 / enemy.GetDistance(), 4);
            double fireVel = 30 - baseFirePow * 2.5;
            long time = (long)(enemy.GetDistance() / fireVel);
            // Calculate the time it will take for the bulllet to reach the enemy tank
            // Predict the future position of the enemy tank based on its current position and velocity

            double predX = enemy.GetPredX(time);
            double predY = enemy.GetPredY(time);
            double bearingAngle = Math.Atan2(predX - X, predY - Y) * (180.0 / Math.PI);

            SetTurnGunRight(NormalizeAngle(bearingAngle - GunHeading));
            if (Math.Abs(GunTurnRemaining) < 20)
            {
                SetFire(baseFirePow);
            }
        }

        public void MoveAndFireGunEnergy()
        {
            // Decreased overall firepower due to my tank's health
            // Calculate the base firepower based on the distance to the enemy
            double baseFirePow = Math.Min(genFirePow - 100 / enemy.GetDistance(), 3);
            // Adjust the base firepower based on factors such as enemy velocity and robot energy level
            double adjustedFirePow = AdjustFirePower(baseFirePow);
            // Calculate the time it will take for the bulllet to reach the enemy tank
            double fireVel = 25 - adjustedFirePow * 2.5;
            long time = (long)(enemy.GetDistance() / fireVel);
            // Predict the future position of the enemy tank based on its current position and velocity
            double predX = enemy.GetPredX(time);
            double predY = enemy.GetPredY(time);
            double bearingAngle = Math.Atan2(predX - X, predY - Y) * (180.0 / Math.PI);
            SetTurnGunRight(NormalizeAngle(bearingAngle - GunHeading));
            // Fire the gun if the gun's turn angle is small enough
            if (Math.Abs(GunTurnRemaining) < 20)
            {
                SetFire(adjustedFirePow);
            }
        }

        private double AdjustFirePower(double baseFirePow)
        {
            // Calculate a velocity factor based on the enemy tank's velocity
            double velocityFactor = enemy.GetVelocity() * 0.1;
            // Apply the velocity factor to adjust the base firepower
            double adjustedFirePow = baseFirePow + velocityFactor;
            // If the robot's energy level is low, reduce the adjusted firepower
            if (Energy < 25)
            {
                adjustedFirePow -= 0.5;
            }
            // Ensure that the adjusted firepower is within a valid range
            adjustedFirePow = Math.Max(0, Math.Min(3, adjustedFirePow));
            return adjustedFirePow;
        }

        // Normalizes angle to be between -180 and 180 degrees
        double NormalizeAngle(double angle)
        {
            angle = (angle + 180) % 360;
            if (angle < 0)
            {
                angle += 360;
            }
            return angle - 180;
        }
        
        // Method to find if my tank is close to a wall based on wallMargin
        public bool IsCloseToWall()
        {
            int wallMargin = 60;
            double battlefieldWidth = BattleFieldWidth;
            double battlefieldHeight = BattleFieldHeight;

            return (X <= wallMargin || X >= battlefieldWidth - wallMargin || Y <= wallMargin || Y >= battlefieldHeight - wallMargin);
        }

        // Method to find if enemy tank is close to my tank based on thresholdDistance
        public bool IsEnemyClose()
        {
            double thresholdDistance = 80;
            return enemy.GetDistance() < thresholdDistance;
        }

        // Returns if my tank is unhealthy based on energy level of 25
        public bool IsUnhealthy()
        {
            if (Energy < 25)
            {
                return true;
            }
            return false;
        }

        // Simply returns if there are multiple enemies or not
        public bool MultipleEnemies()
        {
            return enemiesCount > 0;
        }

        // Inspired by the EnemyBot class from: https://github.com/jd12/Robocode-Scanning
        public class EnemyTank
        {
            // Attributes to store enemy tank's data
            private double bearing;
            private double distance;
            private double heading;
            private String name;
            private double velocity;
            private double x;
            private double y;

            // constructor to initlaize enemy's tank atributes using the NewBot() function
            public EnemyTank()
            {
                NewBot();
            }

            // Method to reset the enemy tank's attributes
            public void NewBot()
            {
                name = "";
                bearing = 0.0;
                distance = 0.0;
                heading = 0.0;
                velocity = 0.0;
                x = 0.0;
                y = 0.0;
            }

            // Method to update enemy tank's attributes based on scanned event
            public void Update(ScannedRobotEvent e, Robot myRobot)
            {
                name = e.Name;
                bearing = e.Bearing;
                distance = e.Distance;
                heading = e.Heading;
                velocity = e.Velocity;
                CalcXY(e, myRobot);
            }

            // Method to calculate the x and y coordinates of the enemy tank
            private void CalcXY(ScannedRobotEvent e, Robot myRobot)
            {
                // Calculate absolute bearing and normalize the bearing
                // Current heading of my robot added to the bearing of the enemy tank from my tank's perspective
                double absBearing = myRobot.Heading + e.Bearing;
                if (absBearing < 0)
                {
                    absBearing += 360;
                }
                // Calculate x and y coordinate using my tank's coordinates calculating the sine of the angle in randians and the cosine of the angle
                x = myRobot.X + Math.Sin(Math.PI * absBearing / 180) * e.Distance;
                y = myRobot.Y + Math.Cos(Math.PI * absBearing / 180) * e.Distance;
            }

            // Getter functions for private variables, enemy tank's attributes
            public double GetBearing() => bearing;
            public double GetDistance() => distance;
            public double GetVelocity() => velocity;
            public String GetName() => name;
            public bool NoEnemy() => string.IsNullOrEmpty(name);
            // Predicted X and Y coordinate calculations. The X coordinate is calculated by adding the product of velocity, time, and the sine of the heading angle (in radians) to the current X coordinate.
            // Similar to the process for the Y coordinate.
            public double GetPredX(long time) => x + velocity * time * Math.Sin(heading * Math.PI / 180);
            public double GetPredY(long time) => y + velocity * time * Math.Cos(heading * Math.PI / 180);
        }

    }
}
