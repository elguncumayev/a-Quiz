using System;

[Serializable]
public class Question
{
    public int id;
    public string category;
    public string question;
    public string correctAnswer;
    public string[] wrongAnswers;

    public Question()
    {
        wrongAnswers = new string[3];
    }

    public Question(int id, string category, string question, string correctAnswer, string wrongAnswer0, string wrongAnswer1, string wrongAnswer2)
    {
        this.id = id;
        this.category = category;
        this.question = question;
        this.correctAnswer = correctAnswer;
        wrongAnswers = new string[3];
        wrongAnswers[0] = wrongAnswer0;
        wrongAnswers[1] = wrongAnswer1;
        wrongAnswers[2] = wrongAnswer2;
    }

    public string Formatted()
    {
        return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", id, category, question, correctAnswer, wrongAnswers[0], wrongAnswers[1], wrongAnswers[2]);
    }

    public static Question Parse(string str)
    {
        string[] splitted = str.Split('|');
        return new Question(int.Parse(splitted[0]), splitted[1], splitted[2], splitted[3], splitted[4], splitted[5], splitted[6]);
    }

    public override string ToString()
    {
        return string.Format("ID : {0}; Category : {1}; Question : {2}; CorrectAnswer : {3}; WrongAnswers : {4}, {5}, {6}", id, category, question, correctAnswer, wrongAnswers[0], wrongAnswers[1], wrongAnswers[2]);
    }
    public override bool Equals(Object obj)
    {
        if (!(obj is Question other))
            return false;
        else
            return question.Equals(other.question);
    }

    public override int GetHashCode()
    {
        return question.GetHashCode();
    }
}
