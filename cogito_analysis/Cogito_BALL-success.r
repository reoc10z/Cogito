### THIS SCRIPT IS GOING TO ANALYZE DATA IN ORDER TO TEST HYPOTHESIS FROM 
### VARIABLE: ball-success

### libraries installed

# library to manage data
library(dplyr)

# library for plots
library(ggpubr)

#library for some statistical tests
library(car)


### read data
input_path = './Downloads/Proyecto - Driving stress/Data/Data_cogito/Processed_data/final_tests/2_filter2/'
input_file_ball = 'ball_data.csv'

in_data_ball <- read.csv( paste(input_path,input_file_ball, sep ='') , sep = ';' )
# Transform none to na
in_data_ball <- na_if(in_data_ball, 'None')
# transform to numeric values
in_data_ball$type_test <- as.factor(in_data_ball$type_test)
in_data_ball[, 6]  <- as.logical(in_data_ball[, 6])
in_data_ball[, c(1, 3:5, 7:8)]  = as.numeric( unlist( in_data_ball[, c(1, 3:5, 7:8)] ) )



# Perform the Chi-Square test. I use this to test dependence. I want to see if $success depends on the $type_test
# H0: The two variables are independent.
# H1: The two variables relate to each other.
# ch-sq has the assumption that a subject belong to only one cell in the contingency table.
# It means we need to transform the data we have from the variable $success

# unique values
levels = unique(in_data_ball$level)
ids = unique(in_data_ball$id_user)
# create empty dataframe
data_success <- data.frame(type_test = character(0), id_user = numeric(0), level = numeric(0), success_60 = logical(0), success_90 = logical(0), success_100 = logical(0) )
for (level in levels) {
  dataLevel = in_data_ball[ in_data_ball$level == level, ]
  for (id in ids) {
    dataUser = dataLevel[dataLevel$id_user == id, ]
    typeTest = unique(dataUser$type_test)
    nSuccess = nrow( dataUser[dataUser$success == TRUE,] )
    nTotal = nrow(dataUser)
    rateSuccess = nSuccess/nTotal
    success_60 = FALSE
    success_90 = FALSE
    success_100 = FALSE
    
    if ( rateSuccess >= 1.0 ){
      success_60 = TRUE # it means user had a success with the ball equal or greater than 60% for the corresponding level
      success_90 = TRUE # it means user had a success with the ball equal or greater than 90% for the corresponding level
      success_100 = TRUE # it means user had a success with the ball equal or greater than 100% for the corresponding lev
    } else if ( rateSuccess >= 0.90 ){
      success_60 = TRUE # it means user had a success with the ball equal or greater than 60% for the corresponding level
      success_90 = TRUE # it means user had a success with the ball equal or greater than 90% for the corresponding level
    } else if ( rateSuccess >= 0.60 ){
      success_60 = TRUE # it means user had a success with the ball equal or greater than 60% for the corresponding level
    }
    
    data_success[nrow(data_success)+1, ] <- c( typeTest, id, level, success_60, success_90, success_100 )
  }
}

# data_success is a dataframe saying if a user was successful in ball movements. Success greater than 60% and greater than 90%
head( data_success[ order( data_success[,'id_user'] ), ] )

# below function computes the chi-square for the variable success in the input dataframe. It will use the variable
# success_60 and success_90
compute_chisq <- function(level, data_test) {
  dataLevel = data_test[ data_test$level==level, ]
  # 60% of success
  cat('>> level:' , level, 'percentage >=60%\n')
  TAB = table( dataLevel$type_test, dataLevel$success_60)
  print(TAB)
  barplot(TAB, beside = T , legend = T, main=paste("Level-",level," . Success >= 60%", sep = "") )
  chi = chisq.test(dataLevel$type_test, dataLevel$success_60, correct = TRUE)
  print(chi)
  
  # # 90% of success
  # cat('>> level:' , level, 'percentage >=90%\n')
  # TAB = table( dataLevel$type_test, dataLevel$success_90)
  # print(TAB)
  # barplot(TAB, beside = T , legend = T, main=paste("Level-",level," . Success >= 90%", sep = "") )
  # chi = chisq.test(dataLevel$type_test, dataLevel$success_90, correct = TRUE)
  # print(chi)
  # 
  # # 1000% of success
  # cat('>> level:' , level, 'percentage >=100%\n')
  # TAB = table( dataLevel$type_test, dataLevel$success_100)
  # print(TAB)
  # barplot(TAB, beside = T , legend = T, main=paste("Level-",level," . Success >= 100%", sep = "") )
  # chi = chisq.test(dataLevel$type_test, dataLevel$success_100, correct = TRUE)
  # print(chi)
  
}

# for level 0 I am expecting BIG p-value. Accepting null hyp: variables are independent, i.e. success is not dependent of test type
compute_chisq(level = 0, data_success)
##### 'cause some groups have nData < 5, i use fisher exact test
##### After reading, I can see the fisher exact test can be used when data are completely unbalanced and some groups have less than 5 items
fisher.test( data_success[ data_success$level==0, ]$type_test, data_success[ data_success$level==0, ]$success_60 )

# for level 1 I am expecting BIG p-value. Accepting null hyp: variables are independent, i.e. success is not dependent of test type
compute_chisq(level = 1, data_success)
##### 'cause some groups have nData < 5, i use fisher exact test
#####
fisher.test( data_success[ data_success$level==1, ]$type_test, data_success[ data_success$level==1, ]$success_60 )

# for level 2 I am expecting SMALL p-value. Accepting null hyp: variables are dependent, i.e. success dependent on test type
compute_chisq(level = 2, data_success)
##### 'cause some groups have nData < 5, i use fisher exact test
#####
fisher.test( data_success[ data_success$level==2, ]$type_test, data_success[ data_success$level==2, ]$success_60 )


## A POSSIBLE CONCLUSION
# The success of doing the task in a manual task with a high cognitive load task is not dependent of the type of stimuli (i.e. test type).
# However, analyze data for timers in ball task. 
# I think, one can see that stimuli type does nos affect the success but the time of the task



